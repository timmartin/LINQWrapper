using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LINQWrapper.DBOperations
{
    /// <summary>
    /// Represents the case where we apply an ElementAt() operator to an expression. We don't parse the
    /// sub-expression, we just store it and, when the time comes, pass it back to the query provider.
    /// This enables us to take advantage of caching of the query.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ElementAtOperation<T> : DBOperation<T> where T : class, new()
    {
        public ElementAtOperation(Expression innerExpression, int offset)
        {
            this.innerExpression = innerExpression;
            this.offset = offset;
        }

        #region DBOperation<T> Members

        public object Execute(LazyDBQueryProvider<T> provider)
        {
            IEnumerable<T> subResult = (IEnumerable<T>)provider.Execute(innerExpression);

            return subResult.ElementAt(offset);
        }

        public void SetSkipValue(int skipValue)
        {
            throw new NotImplementedException();
        }

        public void SetTakeValue(int takeValue)
        {
            throw new NotImplementedException();
        }

        #endregion

        private int offset;
        private Expression innerExpression;
    }
}
