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
            translator.Translate(expression);

            StringBuilder stringBuilder = new StringBuilder();
            builder.BuildExpression(stringBuilder);

            IDbCommand cmd = connection.CreateCommand();

            cmd.CommandText = stringBuilder.ToString();

            foreach (KeyValuePair<string, object> entry in parameters)
            {
                IDbDataParameter newParam = cmd.CreateParameter();
                newParam.ParameterName = entry.Key;
                newParam.Value = entry.Value;
                cmd.Parameters.Add(newParam);
            }

            // TODO: We need to be a bit careful about lifetime here. At the moment the
            // ObjectBuilder is done with the reader as soon as the constructor finishes,
            // but this might not always be the case. We need a more durable solution that
            // doesn't bind the behaviour of this code to the of the ObjectBuilder
            using (IDataReader reader = cmd.ExecuteReader())
            {
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
        }

        private IDbConnection connection;
        private SQLBuilder builder;
        private Dictionary<string, object> parameters;
    }
}
