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
            fromExpressions = new List<string>();
        }

        #region SQLBuilder Members

        public void BuildExpression(StringBuilder builder)
        {
            if (selectExpressions.Count == 0)
            {
                throw new IncompleteQueryException();
            }

            BuildSelectClause(builder);

            BuildFromClause(builder);

            if (whereConstraint != null)
            {
                builder.Append(" WHERE ");

                whereConstraint.BuildExpression(builder);
            }

            BuildLimitClause(builder);

            builder.Append(";");
        }

        public void AddSelectClause(string selectClause)
        {
            selectExpressions.Add(selectClause);
        }

        public void AddFromClause(string fromClause)
        {
            fromExpressions.Add(fromClause);
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

        public void SkipResults(int numResults)
        {
            skipResults = numResults;
        }

        public void TakeResults(int numResults)
        {
            takeResults = numResults;
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

        private void BuildFromClause(StringBuilder builder)
        {
            if (fromExpressions.Count > 0)
            {
                builder.Append(" FROM ");

                bool first = true;

                foreach (string expression in fromExpressions)
                {
                    if (!first)
                    {
                        builder.Append(", ");
                    }

                    builder.Append(expression);
                    first = false;
                }
            }
        }

        private void BuildLimitClause(StringBuilder builder)
        {
            if (skipResults.HasValue && takeResults.HasValue)
            {
                builder.AppendFormat(" LIMIT {0}, {1}", skipResults.Value, takeResults.Value);
            }
            else if (takeResults.HasValue)
            {
                builder.AppendFormat(" LIMIT {0}", takeResults.HasValue);
            }
            
            // TODO: The remaining case can't actually be implemented in MySQL. We can fudge it by setting
            // a skip value and a very large take value, but perhaps we should log a warning?
        }

        #endregion

        #region Private data members

        List<string> selectExpressions;
        List<string> fromExpressions;
        Constraint whereConstraint;
        Nullable<int> skipResults;
        Nullable<int> takeResults;

        #endregion
    }
}
