using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LINQWrapper
{
    /// <summary>
    /// Class that acts a little like StringBuilder, but for building SQL. It copes with the
    /// various parts of the expression being added out of order.
    /// </summary>
    public interface SQLBuilder
    {
        /// <summary>
        /// Build the current state of the SQL expression into the StringBuilder provided
        /// </summary>
        /// <param name="builder"></param>
        void BuildExpression(StringBuilder builder);

        /// <summary>
        /// Add a WHERE clause to the statement
        /// </summary>
        /// <param name="whereClause"></param>
        /// <param name="combine">Boolean operator by which to combine the clause with any
        /// existing WHERE clause. This must be either ExpressionType.And or ExpressionType.Or</param>
        void AddWhereClause(string whereClause, ExpressionType combine);
    }
}
