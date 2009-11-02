using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBOperations
{
    /// <summary>
    /// Operation to execute some SQL that is fixed at the time of construction and can't be modified
    /// later by applying operations such as SetSkipValue().
    /// </summary>
    /// <remarks>
    /// This is used in the optimisations where we want to be able to specify override SQL on a per-expression
    /// basis. In this situation we won't want to make use of lazy query modification to change the expression
    /// later.
    /// </remarks>
    class FixedSQLExecutionOperation<T> : SQLExecutionOperation<T> where T : class, new()
    {
        public FixedSQLExecutionOperation(string sqlCommand)
        {
            this.sqlCommand = sqlCommand;
        }

        #region DBOperation<T> Members

        public override object Execute(LazyDBQueryProvider<T> provider, Dictionary<string, object> parameters)
        {
            using (IDbConnection connection = provider.GetConnection())
            {
                using (IDataReader reader = GetReader(connection, parameters))
                {
                    return new ObjectBuilder<T>(reader);
                }
            }
        }

        internal override IDataReader GetReader(IDbConnection connection, Dictionary<string, object> parameters)
        {
            IDbCommand cmd = connection.CreateCommand();
            cmd.CommandText = sqlCommand;

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
            throw new NotImplementedException();
        }

        public override void SetTakeValue(int takeValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        private string sqlCommand;
    }
}
