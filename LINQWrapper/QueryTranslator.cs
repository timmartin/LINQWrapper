using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using IQToolkit;
using LINQWrapper.DBMapping;
using LINQWrapper.DBOperations;

namespace LINQWrapper
{
    /// <summary>
    /// The query translator turns a LINQ expression into a structure that is closer to that of an
    /// SQL query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class QueryTranslator<T> : ExpressionVisitor where T : class, new()
    {
        internal QueryTranslator(SQLBuilder builder)
        {
            this.builder = builder;
        }

        internal DBOperation<T> Translate(Expression expression, DBOperation<T> operationToDecorate)
        {
            /* We start by setting the operation to be a simple SELECT on the DB. We then decorate this 
             * expression with extra wrappers if we find out that we have to during processing. Structurally,
             * this is a bit mucky since the SELECT ought to be the innermost expression, but we set it
             * in the outermost stage of the recursion. */

            resultantOperation = operationToDecorate;
            
            this.Visit(expression);

            return resultantOperation;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression) e).Operand;
            }

            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable))
            {
                if (m.Method.Name == "OrderBy")
                {
                    this.Visit(m.Arguments[0]);

                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    orderFieldExpression = null;
                    this.Visit(lambda.Body);
                    // Visiting the lambda body should cause us to eventually hit a member access,
                    // at which point the orderFieldName will be populated.

                    if (orderFieldExpression != null)
                    {
                        builder.AddOrderByClause(orderFieldExpression, SortDirection.Ascending);
                    }
                }
                else if (m.Method.Name == "ThenBy")
                {
                    this.Visit(m.Arguments[0]);

                    LambdaExpression lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                    orderFieldExpression = null;
                    this.Visit(lambda.Body);

                    if (orderFieldExpression != null)
                    {
                        builder.AddOrderByClause(orderFieldExpression, SortDirection.Ascending);
                    }
                }
                else if (m.Method.Name == "Count")
                {
                    if (!string.IsNullOrEmpty(CountOverrideSQL))
                    {
                        resultantOperation = new AggregateReadOperation<T>(new FixedSQLExecutionOperation<T>(CountOverrideSQL));
                    }
                    else
                    {
                        builder.AddCountClause();

                        // We can only apply a Count to an SQL execution operation
                        resultantOperation = new AggregateReadOperation<T>((SQLExecutionOperation<T>)resultantOperation);
                    }
                }
                else if (m.Method.Name == "Cast")
                {
                    /* TODO: This code sucks */

                    this.Visit(m.Arguments[0]);

                    Type fromType = typeof(T);
                    Type toType = m.Method.GetGenericArguments()[0];

                    Type castOperationType = typeof(CastOperation<,>).MakeGenericType(new Type[] { fromType, toType });

                    resultantOperation = (DBOperation<T>)Activator.CreateInstance(castOperationType, new object[] { resultantOperation });
                }
                else if (m.Method.Name == "Skip")
                {
                    this.Visit(m.Arguments[0]);

                    if (!(m.Arguments[1] is ConstantExpression))
                    {
                        throw new Exception("Can only apply Skip() to constant expressions");
                    }

                    ConstantExpression skipExpression = (ConstantExpression)m.Arguments[1];

                    if (skipExpression.Type != typeof(int))
                    {
                        throw new Exception("Can only apply Skip to arguments of integer type");
                    }

                    resultantOperation.SetSkipValue((int)skipExpression.Value);
                }
                else if (m.Method.Name == "Take")
                {
                    this.Visit(m.Arguments[0]);

                    if (!(m.Arguments[1] is ConstantExpression))
                    {
                        throw new Exception("Can only apply Take() to constant expressions");
                    }

                    ConstantExpression takeExpression = (ConstantExpression)m.Arguments[1];

                    if (takeExpression.Type != typeof(int))
                    {
                        throw new Exception("Can only apply Take() to arguments of integer type");
                    }

                    resultantOperation.SetTakeValue((int)takeExpression.Value);
                }
                else if (m.Method.Name == "ElementAt")
                {
                    /* We don't attempt to parse the sub-expression, we pass it into the operation,
                     * which will lazily evaluate it by passing the sub-expression back to the
                     * provider when the time comes. Since the provider caches the result, this is
                     * actually more efficient than producing two different (but related) expressions */

                    if (!(m.Arguments[1] is ConstantExpression))
                    {
                        throw new Exception("Can only apply ElementAt() to constant expressions");
                    }

                    ConstantExpression elementAtExpression = (ConstantExpression)m.Arguments[1];

                    if (elementAtExpression.Type != typeof(int))
                    {
                        throw new Exception("Can only apply ElementAt() to arguments of integer type");
                    }

                    resultantOperation = new ElementAtOperation<T>(m.Arguments[0], (int)elementAtExpression.Value);
                }
            }

            return m;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression.NodeType != ExpressionType.Parameter)
            {
                /* At this point, we should recurse until we find a member access that is applied to a parameter */
                this.Visit(m.Expression);
            }

            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                /* We're making an assumption here. We can't get the actual underlying type of the object
                 * we're getting the member of here because we may have upcasted the expression - 
                 * and if we've upcasted, we'll have lost all the nice reflection attributes, even if we've
                 * preserved the necessary fields. Therefore we assume that we can only do a member access on
                 * an instance if it's an instance of the class we're querying for - no other instance ought
                 * to be in scope. I'm pretty sure this can go wrong, though */
                currentlyProcessingType = typeof(T);
            }

            if (currentlyProcessingType != null)
            {
                string parameterName = m.Member.Name;

                var targetProperties = from property in currentlyProcessingType.GetProperties()
                                       where property.Name == parameterName
                                       select property;

                if (!targetProperties.Any())
                {
                    throw new NotSupportedException(string.Format("Attempted to use unsupported property '{0}'", parameterName));
                }

                PropertyInfo targetProperty = targetProperties.Single();

                var fieldAttributes = from attr in targetProperty.GetCustomAttributes(typeof(FieldMappingAttribute), false)
                                      select (FieldMappingAttribute)attr;

                if (!fieldAttributes.Any())
                {
                    var recursiveAttributes = from attr in targetProperty.GetCustomAttributes(typeof(RecursiveFetchAttribute), false)
                                              select (RecursiveFetchAttribute)attr;

                    if (!recursiveAttributes.Any())
                    {
                        throw new NotSupportedException(string.Format("Attempted to order by a property '{0}' that doesn't map to the database", parameterName));
                    }

                    /* If the attribute has also specified an override type we should use this type for preference */
                    currentlyProcessingType = recursiveAttributes.Single().TypeToConstruct;
                    if (currentlyProcessingType == null)
                    {
                        currentlyProcessingType = targetProperty.PropertyType;
                    }
                }
                else
                {
                    FieldMappingAttribute fieldMapAttr = fieldAttributes.Single();

                    var modifierAttributes = from attr in targetProperty.GetCustomAttributes(typeof(OrderByModifierAttribute), false)
                                             select attr;

                    if (modifierAttributes.Any())
                    {
                        OrderByModifierAttribute modifier = (OrderByModifierAttribute)modifierAttributes.Single();
                        orderFieldExpression = modifier.GetOrderByExpression(fieldMapAttr.UniqueFieldAlias);
                    }
                    else
                    {
                        orderFieldExpression = fieldMapAttr.UniqueFieldAlias;
                    }
                }

                return m;
            }

            throw new NotSupportedException(string.Format("Member {0} is not recognised", m.Member.Name));
        }

        public string CountOverrideSQL
        {
            get;
            set;
        }

        private SQLBuilder builder;

        private string orderFieldExpression;

        private Type currentlyProcessingType;

        private DBOperation<T> resultantOperation;
    }
}
