using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using IQToolkit;

namespace LINQWrapper
{
    public class LazyDBQueryProvider<T> : QueryProvider where T : class, new()
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
            QueryTranslator<T> translator = new QueryTranslator<T>(builder);
            translator.Translate(expression);

            StringBuilder stringBuilder = new StringBuilder();
            builder.BuildExpression(stringBuilder);

            IDbCommand cmd = connection.CreateCommand();

            cmd.CommandText = stringBuilder.ToString();

            IDataReader reader = cmd.ExecuteReader();

            if (translator.Aggregate)
            {
                // We're going to assume this is a count query, since this is the only
                // aggregate we support at the moment. This needs to be tidied up.
                reader.Read();
                return int.Parse(reader["numrows"].ToString());
            }
            else
            {
                return new ObjectBuilder<T>(reader);
            }
        }

        private IDbConnection connection;
        private SQLBuilder builder;
    }
}
