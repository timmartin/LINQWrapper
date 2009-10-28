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
        }

        public override string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }

        public override object Execute(Expression expression)
        {
            SQLBuilder clonedBuilder = (SQLBuilder) builder.Clone();
            QueryTranslator<T> translator = new QueryTranslator<T>(clonedBuilder);

            using (IDbConnection connection = connectionProvider())
            {
                SQLExecutionOperation<T> innerOperation = new SQLExecutionOperation<T>(connection, clonedBuilder, parameters);

                DBOperation operation = translator.Translate(expression, innerOperation);

                return operation.Execute();
            }
        }

        private Func<IDbConnection> connectionProvider;
        private SQLBuilder builder;
        private Dictionary<string, object> parameters;
    }
}
