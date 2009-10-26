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
    public interface SQLBuilder : ICloneable
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
        /// Add all the necessary entries to the SELECT clause for a particular type, which must have
        /// attributes set on its properties so the field names can be determined
        /// </summary>
        /// <param name="tableName">The table name (or alias) to select from</param>
        /// <param name="typeToSelect"></param>
        void AddSelectTypeClause(string tableName, Type typeToSelect);

        /// <summary>
        /// Add an expression to the FROM clause, i.e. a table to select from (sub-selects are being
        /// ignored for the moment)
        /// </summary>
        /// <param name="fromClause"></param>
        void AddFromClause(string fromClause);

        /// <summary>
        /// Add a JOIN clause to the query. Before the expression is built, you must also submit a plain
        /// FROM clause.
        /// </summary>
        /// <remarks>
        /// There's an abstraction leak here, in that we allow anything to be specified as the join keyword,
        /// even a DBMS-specific join type (indeed, that's one of the main reasons for having this as a free
        /// string). There's no other way to support this without preventing the user from making DBMS-specific
        /// calls when they need to; we assume they know what they're doing.
        /// </remarks>
        /// <param name="joinKeyword">The keyword to use in the join, e.g. "JOIN", "LEFT JOIN", etc.</param>
        /// <param name="table"></param>
        /// <param name="condition"></param>
        void AddJoinClause(string joinKeyword, string table, string condition);

        /// <summary>
        /// Add a WHERE clause to the statement
        /// </summary>
        /// <param name="whereClause"></param>
        /// <param name="combine">Boolean operator by which to combine the clause with any
        /// existing WHERE clause. This must be either ExpressionType.And or ExpressionType.Or</param>
        void AddWhereClause(string whereClause, ExpressionType combine);

        /// <summary>
        /// Add an order by clause to the statement (combines with previous order by commands, 
        /// orderings will be applied in the order they are received)
        /// </summary>
        /// <param name="orderBy"></param>
        /// <param name="direction"></param>
        void AddOrderByClause(string orderBy, SortDirection direction);

        /// <summary>
        /// Causes the statement to return a count of the results, rather than a set of the results
        /// themselves
        /// </summary>
        void AddCountClause();

        /// <summary>
        /// Set the number of results to skip over in the ordered result set
        /// </summary>
        /// <param name="numResults"></param>
        void SkipResults(int numResults);

        /// <summary>
        /// Set the number of results to return
        /// </summary>
        /// <param name="numResults"></param>
        void TakeResults(int numResults);
    }
}
