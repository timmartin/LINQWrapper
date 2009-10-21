using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;

using LINQWrapper.Exceptions;

namespace LINQWrapper.Tests
{
    [TestFixture]
    public class MySQLBuilderTests
    {
        [Test]
        public void BuildExpression_EmptyExpression()
        {
            MySQLBuilder builder = new MySQLBuilder(); ;
            StringBuilder sBuilder = new StringBuilder();

            Assert.Throws(typeof(IncompleteQueryException), () => builder.BuildExpression(sBuilder));
        }

        [Test]
        public void BuildExpression_TrivialSelect()
        {

        }
    }
}
