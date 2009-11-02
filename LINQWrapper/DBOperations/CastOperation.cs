using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBOperations
{
    public class CastOperation<FromType, ToType> : DBOperation<FromType> where FromType : class, new()
    {
        public CastOperation(SQLExecutionOperation<FromType> innerOperation)
        {
            this.innerOperation = innerOperation;
        }

        #region DBOperation Members

        public object Execute(LazyDBQueryProvider<FromType> provider)
        {
            return ((IEnumerable<FromType>)innerOperation.Execute(provider)).Cast<ToType>();
        }

        public void SetSkipValue(int skipValue)
        {
            innerOperation.SetSkipValue(skipValue);
        }

        public void SetTakeValue(int takeValue)
        {
            innerOperation.SetTakeValue(takeValue);
        }

        #endregion

        private SQLExecutionOperation<FromType> innerOperation;
    }
}
