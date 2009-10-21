using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using IQToolkit;

namespace LINQWrapper
{
    internal class QueryTranslator : ExpressionVisitor
    {
        internal QueryTranslator(SQLBuilder builder)
        {
            this.builder = builder;
        }

        internal void Translate(Expression expression)
        {
            this.Visit(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "OrderBy")
            {
                Console.WriteLine("Got an orderby call");
            }

            return m;
        }

        private SQLBuilder builder;
    }
}
