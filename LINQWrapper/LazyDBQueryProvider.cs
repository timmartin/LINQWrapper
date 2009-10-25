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
        public LazyDBQueryProvider(IDbConnection connection, SQLBuilder builder, Dictionary<string, object> parameters)
        {
            this.connection = connection;
            this.builder = builder;
            this.parameters = parameters;
        }

        public override string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }

        public override object Execute(Expression expression)
        {
            // TODO: Clone the builder here for repeatability?
            QueryTranslator<T> translator = new QueryTranslator<T>(builder);

            SQLExecutionOperation<T> innerOperation = new SQLExecutionOperation<T>(connection, builder, parameters);

            DBOperation operation = translator.Translate(expression, innerOperation);

            return operation.Execute();
        }

        private IDbConnection connection;
        private SQLBuilder builder;
        private Dictionary<string, object> parameters;
    }
}
