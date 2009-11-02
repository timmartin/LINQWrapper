using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBOperations
{
    class SQLBuilderExecutionOperation<T> : SQLExecutionOperation<T> where T : class, new()
    {
        public SQLBuilderExecutionOperation(Func<IDbConnection> connectionProvider, SQLBuilder sqlBuilder)
        {
            this.connectionProvider = connectionProvider;
            this.sqlBuilder = sqlBuilder;
        }

        #region DBOperation Members

        public override object Execute(LazyDBQueryProvider<T> provider, Dictionary<string, object> parameters)
        {
            // TODO: We need to be a bit careful about lifetime here. At the moment the
            // ObjectBuilder is done with the reader as soon as the constructor finishes,
            // but this might not always be the case. We need a more durable solution that
            // doesn't bind the behaviour of this code to the of the ObjectBuilder
            using (IDbConnection connection = connectionProvider())
            {
                using (IDataReader reader = GetReader(connection, parameters))
                {
                    return new ObjectBuilder<T>(reader);
                }
            }
        }

        #endregion

        internal override IDataReader GetReader(IDbConnection connection, Dictionary<string, object> parameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            sqlBuilder.BuildExpression(stringBuilder);

            IDbCommand cmd = connection.CreateCommand();

            cmd.CommandText = stringBuilder.ToString();

            foreach (KeyValuePair<string, object> entry in parameters)
            {
                IDbDataParameter newParam = cmd.CreateParameter();
                newParam.ParameterName = entry.Key;
                newParam.Value = entry.Value;
                cmd.Parameters.Add(newParam);
            }

            return cmd.ExecuteReader();
        }

        public override void SetSkipValue(int skipValue)
        {
            sqlBuilder.SkipResults(skipValue);
        }

        public override void SetTakeValue(int takeValue)
        {
            sqlBuilder.TakeResults(takeValue);
        }

        private Func<IDbConnection> connectionProvider;
        private SQLBuilder sqlBuilder;
    }
}
