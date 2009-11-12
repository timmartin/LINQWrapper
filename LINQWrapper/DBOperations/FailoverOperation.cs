using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBOperations
{
    /// <summary>
    /// Allows us to combine two or more operations into single composite operation where first one operation
    /// is attempted, then another.
    /// </summary>
    /// <remarks>
    /// This was originally useful because it allows us to implement 
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class FailoverOperation<T> : DBOperation<T> where T : class, new()
    {
        public FailoverOperation()
        {
            subOperations = new List<DBOperation<T>>();
        }

        public void AddOperation(DBOperation<T> newOperation)
        {
            subOperations.Add(newOperation);
        }

        #region DBOperation<T> Members

        public object Execute(LazyDBQueryProvider<T> provider, Dictionary<string, object> parameters)
        {
            foreach (DBOperation<T> currentOperation in subOperations)
            {
                try
                {
                    return currentOperation.Execute(provider, parameters);
                }
                catch (ApplicationException)
                    /* TODO: This should probably be a more specific exception type */
                {
                }
            }

            /* TODO: Should we throw an exception here? */
            return null;
        }

        public void SetSkipValue(int skipValue)
        {
            foreach (DBOperation<T> currentOperation in subOperations)
            {
                currentOperation.SetSkipValue(skipValue);
            }
        }

        public void SetTakeValue(int takeValue)
        {
            foreach (DBOperation<T> currentOperation in subOperations)
            {
                currentOperation.SetTakeValue(takeValue);
            }
        }

        #endregion

        private List<DBOperation<T>> subOperations;
    }
}
