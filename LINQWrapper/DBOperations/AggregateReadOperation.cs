using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using LINQWrapper.Exceptions;

namespace LINQWrapper.DBOperations
{
    /// <summary>
    /// This wraps an ordinary SQL execution operation, and instead of treating the result as a
    /// list of objects it treats it as an aggregate query.
    /// </summary>
    /// <remarks>
    /// Currently this only supports COUNT queries
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class AggregateReadOperation<T> : DBOperation<T> where T : class, new()
    {
        public AggregateReadOperation(SQLExecutionOperation<T> innerOperation)
        {
            this.innerOperation = innerOperation;
        }

        public object Execute(LazyDBQueryProvider<T> provider, Dictionary<string, object> parameters)
        {
            /* At the moment, we assume we have a COUNT(*) query (with no GROUP BY) since that's all
             * we support */

            using (IDbConnection connection = provider.GetConnection())
            {
                using (IDataReader reader = innerOperation.GetReader(connection, parameters))
                {
                    if (!reader.Read())
                    {
                        throw new BadResultSetException("Expected to have at least one line in result set for Count() query");
                    }

                    return int.Parse(reader["numrows"].ToString());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// We don't support Skip() and Take() expressions on aggregates at the moment, because they
        /// are meaningless without support for GroupBy() (which we also don't have yet). However, 
        /// technically the message that they are meaningless is an exaggeration.
        /// </remarks>
        /// <param name="skipValue"></param>
        public void SetSkipValue(int skipValue)
        {
            throw new Exception("Applying Skip() to an aggregate operation is meaningless");
        }

        public void SetTakeValue(int takeValue)
        {
            throw new Exception("Applying Take() to an aggregate operation is meaningless");
        }

        private SQLExecutionOperation<T> innerOperation;
    }
}
