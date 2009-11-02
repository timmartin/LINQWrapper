using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using LINQWrapper;

namespace LINQWrapper.DBOperations
{
    public abstract class SQLExecutionOperation<T> : DBOperation<T> where T : class, new()
    {
        internal abstract IDataReader GetReader(IDbConnection connection, Dictionary<string, object> parameters);

        #region DBOperation<T> Members

        public abstract object Execute(LazyDBQueryProvider<T> provider, Dictionary<string, object> parameters);

        public abstract void SetSkipValue(int skipValue);

        public abstract void SetTakeValue(int takeValue);

        #endregion
    }
}
