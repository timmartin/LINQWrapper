using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper
{
    internal class ObjectBuilder<T> : IEnumerable<T>, IEnumerable
    {
        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator();
        }

        #endregion

        class Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            #region IEnumerator<T> Members

            public T Current
            {
                get { return default(T); }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return null; }
            }

            public bool MoveNext()
            {
                return false;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            #endregion
        }
    }
}
