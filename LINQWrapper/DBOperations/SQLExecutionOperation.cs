using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using LINQWrapper;

namespace LINQWrapper.DBOperations
{
    public class SQLExecutionOperation<T> : DBOperation where T : class, new()
    {
        public SQLExecutionOperation(IDbConnection conn, SQLBuilder sqlBuilder, Dictionary<string, object> parameters)
        {
            this.connection = conn;
            this.sqlBuilder = sqlBuilder;
            this.parameters = parameters;
        }

        #region DBOperation Members

        public object Execute()
        {
            // TODO: We need to be a bit careful about lifetime here. At the moment the
            // ObjectBuilder is done with the reader as soon as the constructor finishes,
            // but this might not always be the case. We need a more durable solution that
            // doesn't bind the behaviour of this code to the of the ObjectBuilder
            using (IDataReader reader = GetReader())
            {
                return new ObjectBuilder<T>(reader);
            }
        }

        #endregion

        internal IDataReader GetReader()
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

        private IDbConnection connection;
        private SQLBuilder sqlBuilder;
        private Dictionary<string, object> parameters;
    }
}
