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
        /// Add an entry to the SELECT clause, i.e. the list of expressions to be selected. It is intended
        /// that the string to be added is a single atomic expression (no commas), though an expression 
        /// with commas in it will probably work for the time being.
        /// </summary>
        /// <param name="selectClause"></param>
        void AddSelectClause(string selectClause);

        /// <summary>
        /// Add an expression to the FROM clause, i.e. a table to select from (sub-selects are being
        /// ignored for the moment)
        /// </summary>
        /// <param name="fromClause"></param>
        void AddFromClause(string fromClause);

        /// <summary>
        /// Add a WHERE clause to the statement
        /// </summary>
        /// <param name="whereClause"></param>
        /// <param name="combine">Boolean operator by which to combine the clause with any
        /// existing WHERE clause. This must be either ExpressionType.And or ExpressionType.Or</param>
        void AddWhereClause(string whereClause, ExpressionType combine);
    }
}
