using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using LINQWrapper.Exceptions;
using LINQWrapper.SQLExpressions;

namespace LINQWrapper
{
    /// <summary>
    /// Builds SQL strings for the MySQL dialect
    /// </summary>
    public class MySQLBuilder : SQLBuilder
    {
        public MySQLBuilder()
        {
            selectExpressions = new List<string>();
        }

        #region SQLBuilder Members

        public void BuildExpression(StringBuilder builder)
        {
            if (selectExpressions.Count == 0)
            {
                throw new IncompleteQueryException();
            }

            BuildSelectClause(builder);

            if (whereConstraint != null)
            {
                builder.Append(" WHERE ");

                whereConstraint.BuildExpression(builder);
            }

            builder.Append(";");
        }

        public void AddSelectClause(string selectClause)
        {
            selectExpressions.Add(selectClause);
        }

        public void AddWhereClause(string whereClause, ExpressionType combine)
        {
            if (string.IsNullOrEmpty(whereClause))
            {
                throw new ArgumentException("whereClause");
            }

            if ((combine != ExpressionType.And)
                && (combine != ExpressionType.Or))
            {
                throw new ArgumentOutOfRangeException("Combine operator must be AND or OR");
            }

            if (whereConstraint == null)
            {
                whereConstraint = new AtomicConstraint(whereClause);
            }
            else
            {
                whereConstraint = whereConstraint.CombineConstraint(new AtomicConstraint(whereClause), combine);
            }
        }

        #endregion

        #region Private methods

        private void BuildSelectClause(StringBuilder builder)
        {
            builder.Append("SELECT ");

            bool first = true;

            foreach (string expression in selectExpressions)
            {
                if (!first)
                {
                    builder.Append(", ");
                }

                builder.Append(expression);
                first = false;
            }
        }

        #endregion

        #region Private data members

        List<string> selectExpressions;
        Constraint whereConstraint;

        #endregion
    }
}
