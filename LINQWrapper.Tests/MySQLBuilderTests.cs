using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using NUnit.Framework;

using LINQWrapper.Exceptions;

using LINQWrapper.Tests.TestTypes;

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

            Assert.AreEqual("SELECT DISTINCT 1;", sBuilder.ToString());
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

            Assert.AreEqual("SELECT DISTINCT 1 WHERE  1 = 0 ;", stringBuilder.ToString());
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

            Assert.AreEqual("SELECT DISTINCT name FROM employees WHERE  id=42 ;", stringBuilder.ToString());
        }

        /// <summary>
        /// Build an SQL expression with a limit on the number of results to return
        /// </summary>
        [Test]
        public void BuildExpression_Limit()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.SkipResults(100);
            sqlBuilder.TakeResults(10);

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT DISTINCT id FROM employees LIMIT 100, 10;", stringBuilder.ToString());
        }

        /// <summary>
        /// Build an SQL statement with an ORDER BY clause
        /// </summary>
        [Test]
        public void BuildExpression_OrderBy()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.AddOrderByClause("name", SortDirection.Descending);

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT DISTINCT id FROM employees ORDER BY name DESC;", stringBuilder.ToString());
        }

        /// <summary>
        /// Build an SQL statement with a COUNT() expression (the original SELECT expression
        /// being overridden)
        /// </summary>
        [Test]
        public void BuildExpression_Count()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id");
            sqlBuilder.AddSelectClause("name");
            sqlBuilder.AddFromClause("employees");

            sqlBuilder.AddCountClause();

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT COUNT(*) AS numrows FROM employees;", stringBuilder.ToString());
        }

        /// <summary>
        /// Check that we can build an expression using the JOIN format JOIN bar ON bar.id = foo.id
        /// </summary>
        [Test]
        public void BuildExpression_ANSIJoin()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id");
            sqlBuilder.AddFromClause("foo");
            sqlBuilder.AddJoinClause("LEFT JOIN", "bar", "bar.id = foo.id");

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT DISTINCT id FROM foo LEFT JOIN bar ON bar.id = foo.id;", stringBuilder.ToString());
        }

        /// <summary>
        /// Check that we can add all fields to the SELECT clause based only on a type (and inferring
        /// from the annotations on the type)
        /// </summary>
        [Test]
        public void BuildExpression_TypeSelect()
        {
            MySQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectTypeClause("employees", typeof(Employee));
            sqlBuilder.AddFromClause("employees");

            StringBuilder stringBuilder = new StringBuilder();

            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT DISTINCT employees.id AS employee_id, employees.name AS employee_name FROM employees;", stringBuilder.ToString());
        }

        [Test]
        public void Clone_SimpleTest()
        {
            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("name");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.AddWhereClause("id=42", ExpressionType.And);

            SQLBuilder clonedBuilder = (SQLBuilder) sqlBuilder.Clone();

            sqlBuilder.AddCountClause();

            StringBuilder stringBuilder = new StringBuilder();
            sqlBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT COUNT(*) AS numrows FROM employees WHERE  id=42 ;", stringBuilder.ToString());

            stringBuilder = new StringBuilder();

            clonedBuilder.BuildExpression(stringBuilder);

            Assert.AreEqual("SELECT DISTINCT name FROM employees WHERE  id=42 ;", stringBuilder.ToString());
        }
    }
}
