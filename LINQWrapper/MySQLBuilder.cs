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
        }

        #endregion

        #region Private data members

        List<string> selectExpressions;

        #endregion
    }
}
