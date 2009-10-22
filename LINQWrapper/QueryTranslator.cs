using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using IQToolkit;
using LINQWrapper.DBMapping;

namespace LINQWrapper
{
    internal class QueryTranslator<T> : ExpressionVisitor
    {
        internal QueryTranslator(SQLBuilder builder)
        {
            this.builder = builder;
        }

        internal void Translate(Expression expression)
        {
            this.Visit(expression);
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
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderBy")
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
    }
}
