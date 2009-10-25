using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

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
    public class AggregateReadOperation<T> : DBOperation where T : class, new()
    {
        public AggregateReadOperation(SQLExecutionOperation<T> innerOperation)
        {
            this.innerOperation = innerOperation;
        }

        public object Execute()
        {
            /* At the moment, we assume we have a COUNT(*) query (with no GROUP BY) since that's all
             * we support */

            using (IDataReader reader = innerOperation.GetReader())
            {
                reader.Read();
                return int.Parse(reader["numrows"].ToString());
            }
        }

        private SQLExecutionOperation<T> innerOperation;
    }
}
