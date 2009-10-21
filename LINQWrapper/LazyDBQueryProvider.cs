using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using IQToolkit;

namespace LINQWrapper
{
    public class LazyDBQueryProvider : QueryProvider
    {
        public LazyDBQueryProvider(IDbConnection connection, SQLBuilder builder)
        {
            this.connection = connection;
            this.builder = builder;
        }

        public override string GetQueryText(Expression expression)
        {
            throw new NotImplementedException();
        }

        public override object Execute(Expression expression)
        {
            // TODO: Clone the builder here for repeatability?
            QueryTranslator translator = new QueryTranslator(builder);
            translator.Translate(expression);

            StringBuilder stringBuilder = new StringBuilder();
            builder.BuildExpression(stringBuilder);

            IDbCommand cmd = connection.CreateCommand();

            cmd.CommandText = stringBuilder.ToString();

            return new ObjectBuilder<int>();
        }

        private IDbConnection connection;
        private SQLBuilder builder;
    }
}
