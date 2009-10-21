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
        /// <summary>
        /// We have to have at least one expression in the SELECT clause, or we can't generate valid SQL
        /// </summary>
        [Test]
        public void BuildExpression_EmptyExpression()
        {
            MySQLBuilder builder = new MySQLBuilder();
            StringBuilder sBuilder = new StringBuilder();

            Assert.Throws(typeof(IncompleteQueryException), () => builder.BuildExpression(sBuilder));
        }

        /// <summary>
        /// A SELECT clause with just a single constant expression is valid
        /// </summary>
        [Test]
        public void BuildExpression_TrivialSelect()
        {
            MySQLBuilder builder = new MySQLBuilder();
            builder.AddSelectClause("1");

            StringBuilder sBuilder = new StringBuilder();
            builder.BuildExpression(sBuilder);

            Assert.AreEqual("SELECT 1;", sBuilder.ToString());
        }
    }
}
