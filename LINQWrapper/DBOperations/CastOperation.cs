using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBOperations
{
    public class CastOperation<FromType, ToType> : DBOperation where FromType : class, new()
    {
        public CastOperation(SQLExecutionOperation<FromType> innerOperation)
        {
            this.innerOperation = innerOperation;
        }

        #region DBOperation Members

        public object Execute()
        {
            return ((IEnumerable<FromType>)innerOperation.Execute()).Cast<ToType>();
        }

        #endregion

        private SQLExecutionOperation<FromType> innerOperation;
    }
}
