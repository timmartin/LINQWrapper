using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBOperations
{
    /// <summary>
    /// When we have translated a LINQ expression, the output will be one of these. Essentially
    /// it's a promise that at a later date we can execute the operation and get a result out
    /// (which will be of the appropriate type, given the LINQ expression that was applied).
    /// All the necessary work (calling the DB, extracting data, casting etc.) will be taken care
    /// of within this class.
    /// </summary>
    public interface DBOperation<T> where T : class, new()
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider">The query provider is passed in, because in certain cases we
        /// want to do a recursive call to the provider to execute a sub-expression.</param>
        /// <returns></returns>
        object Execute(LazyDBQueryProvider<T> provider);

        void SetSkipValue(int skipValue);

        void SetTakeValue(int takeValue);
    }
}
