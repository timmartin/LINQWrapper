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
    internal class QueryTranslator<T> : ExpressionVisitor where T : class, new()
    {
        internal QueryTranslator(SQLBuilder builder)
        {
            this.builder = builder;
        }

        internal DBOperation Translate(Expression expression, DBOperation operationToDecorate)
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

                    orderFieldName = null;
                    this.Visit(lambda.Body);
                    // Visiting the lambda body should cause us to eventually hit a member access,
                    // at which point the orderFieldName will be populated.

                    if (orderFieldName != null)
                    {
                        builder.AddOrderByClause(orderFieldName, SortDirection.Ascending);
                    }
                }
                else if (m.Method.Name == "Count")
                {
                    builder.AddCountClause();

                    // We can only apply a Count to an SQL execution operation
                    resultantOperation = new AggregateReadOperation<T>((SQLExecutionOperation<T>)resultantOperation);
                }
                else if (m.Method.Name == "Cast")
                {
                    /* TODO: This code sucks */

                    Type fromType = typeof(T);
                    Type toType = m.Method.GetGenericArguments()[0];

                    Type castOperationType = typeof(CastOperation<,>).MakeGenericType(new Type[] { fromType, toType });

                    resultantOperation = (DBOperation) Activator.CreateInstance(castOperationType, new object[] { resultantOperation });
                }
                else if (m.Method.Name == "Skip")
                {
                    this.Visit(m.Arguments[0]);

                    if (!(resultantOperation is SQLExecutionOperation<T>))
                    {
                        throw new Exception("Can only apply Skip() directly to a database select operation");
                    }

                    SQLExecutionOperation<T> operation = (SQLExecutionOperation<T>)resultantOperation;

                    if (!(m.Arguments[1] is ConstantExpression))
                    {
                        throw new Exception("Can only apply Skip() to constant expressions");
                    }

                    ConstantExpression skipExpression = (ConstantExpression)m.Arguments[1];

                    if (skipExpression.Type != typeof(int))
                    {
                        throw new Exception("Can only apply Skip to arguments of integer type");
                    }

                    operation.SetSkipValue((int) skipExpression.Value);
                }
                else if (m.Method.Name == "Take")
                {
                    this.Visit(m.Arguments[0]);

                    if (!(resultantOperation is SQLExecutionOperation<T>))
                    {
                        throw new Exception("Can only apply Take() directly to a database select operation");
                    }

                    SQLExecutionOperation<T> operation = (SQLExecutionOperation<T>)resultantOperation;

                    if (!(m.Arguments[1] is ConstantExpression))
                    {
                        throw new Exception("Can only apply Take() to constant expressions");
                    }

                    ConstantExpression takeExpression = (ConstantExpression)m.Arguments[1];

                    if (takeExpression.Type != typeof(int))
                    {
                        throw new Exception("Can only apply Take() to arguments of integer type");
                    }

                    operation.SetTakeValue((int)takeExpression.Value);
                }
            }

            return m;
        }

        protected override Expression VisitMemberAccess(MemberExpression m)
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {
                string parameterName = m.Member.Name;

                var targetProperties = from property in typeof(T).GetProperties()
                                       where property.Name == parameterName
                                       select property;

                if (!targetProperties.Any())
                {
                    throw new NotSupportedException(string.Format("Attempted to use unsupported property '{0}'", parameterName));
                }

                PropertyInfo targetProperty = targetProperties.First(); // If there's more than one, something very strange has happened

                var fieldNames = from attr in targetProperty.GetCustomAttributes(typeof(FieldMappingAttribute), false)
                                 select ((FieldMappingAttribute)attr).FieldName;

                if (!fieldNames.Any())
                {
                    throw new NotSupportedException(string.Format("Attempted to order by a property '{0}' that doesn't map to the database", parameterName));
                }

                orderFieldName = fieldNames.First();

                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }

        private SQLBuilder builder;

        private string orderFieldName;

        private DBOperation resultantOperation;
    }
}
