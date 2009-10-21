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
        #region SQLBuilder Members

        public void BuildExpression(StringBuilder builder)
        {
            throw new IncompleteQueryException();
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
    }
}
