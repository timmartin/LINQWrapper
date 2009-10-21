using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        /// <summary>
        /// A statement with a constant SELECT clause and constant WHERE clauses is technically valid,
        /// although it is rather pointless in practice
        /// </summary>
        [Test]
        public void BuildExpression_TrivialSelectTrivialWhere()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("1");
            sqlBuilder.AddWhereClause("1 = 0", ExpressionType.Or);

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT 1 WHERE  1 = 0 ;", stringBuilder.ToString());
        }

        /// <summary>
        /// Combine the SELECT, FROM and WHERE clauses
        /// </summary>
        [Test]
        public void BuildExpression_SelectFromWhere()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("name");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.AddWhereClause("id=42", ExpressionType.And);

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT name FROM employees WHERE  id=42 ;", stringBuilder.ToString());
        }
    }
}
