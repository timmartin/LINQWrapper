using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IQToolkit;

namespace LINQWrapper
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// I believe this class may actually be unnecessary
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    public class LazyDBQuery<T> : Query<T> where T : class, new()
    {
        LazyDBQuery(LazyDBQueryProvider<T> provider, SQLBuilder builder) :
            base(provider)
        {
        }
    }
}
