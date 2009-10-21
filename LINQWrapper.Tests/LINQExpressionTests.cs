using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using IQToolkit;
using LINQWrapper;

namespace LINQWrapper.Tests
{
    [TestFixture]
    class LINQExpressionTests
    {
        /// <summary>
        /// Check that we can write a LINQ expression - I'll be happy just to get it to <em>compile</em>
        /// at the minute.
        /// </summary>
        [Test]
        public void BasicExpressionTest()
        {
            LazyDBQueryProvider provider = new LazyDBQueryProvider();

            Query<int> myQuery = new Query<int>(provider);

            Assert.Throws(typeof(NotImplementedException), () => myQuery.Count());
        }
    }
}
