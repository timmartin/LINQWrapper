using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using IQToolkit;
using LINQWrapper.DBOperations;

namespace LINQWrapper
{
    public class LazyDBQueryProvider<T> : QueryProvider where T : class, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <para>
        /// Originally, this constructor took a DB connection as a parameter. However, this makes it difficult
        /// to control the lifetime of the connection, since it may need to last outside the scope in which the
        /// query is created. We can't use IDisposable because clients will handle queries via an
        /// IQueryable reference, and this can't be disposed.
        /// </para>
        /// </remarks>
        /// <param name="connectionString">Function that can provide a database connection</param>
        /// <param name="builder"></param>
        /// <param name="parameters"></param>
        public LazyDBQueryProvider(Func<IDbConnection> connectionProvider, SQLBuilder builder, Dictionary<string, object> parameters)
        {
            this.connectionProvider = connectionProvider;
            this.builder = builder;
            this.parameters = parameters;

            this.cache = new Dictionary<Expression, object>();
        }

        public override string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }

        public override object Execute(Expression expression)
        {
            if (cache.ContainsKey(expression))
            {
                return cache[expression];
            }

            SQLBuilder clonedBuilder = (SQLBuilder)builder.Clone();
            QueryTranslator<T> translator = new QueryTranslator<T>(clonedBuilder);
            if (!string.IsNullOrEmpty(optimisedCountExpression))
            {
                translator.CountOverrideSQL = optimisedCountExpression;
            }

            /* When we create the inner operation, we provide it a connection provider rather than 
             * a connection. This is because the inner operation won't necessarily get executed - if
             * our expression can actually be remapped onto a simpler expression that we already have
             * in the cache, there's no point running a new query. This happens in particular with the
             * ElementAt() expression: there's no point implementing it any way other than by 
             * executing the child expression as SQL and then doing the index into the list with LINQ
             * to entities */
            SQLExecutionOperation<T> innerOperation = new SQLBuilderExecutionOperation<T>(connectionProvider, clonedBuilder);

            DBOperation<T> operation = translator.Translate(expression, innerOperation);

            object result = operation.Execute(this, parameters);
            cache[expression] = result;
            return result;
        }

        /// <summary>
        /// If we have an optimised form of SQL that we can run in the special case of a Count() query, this
        /// specifies what it is.
        /// </summary>
        /// <remarks>
        /// We don't really want a method as specific as this. It would be nice if we could have a method for
        /// specifying an optimisation on a per-expression basis, but generating arbitrary lambda expressions
        /// that match the required lambda expressions we will encounter later during processing is beyond
        /// me at the moment.
        /// </remarks>
        /// <param name="sql"></param>
        public void SetOptimisedCountExpression(string sql)
        {
            optimisedCountExpression = sql;
        }

        internal IDbConnection GetConnection()
        {
            return connectionProvider();
        }

        private Func<IDbConnection> connectionProvider;
        private SQLBuilder builder;
        private Dictionary<string, object> parameters;

        private Dictionary<Expression, object> cache;
        private string optimisedCountExpression;
    }
}
