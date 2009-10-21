using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using NMock2;
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
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();

            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT id FROM employees;");

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectClause("id");
            builder.AddFromClause("employees");

            LazyDBQueryProvider provider = new LazyDBQueryProvider(mockConnection, builder);

            Query<int> myQuery = new Query<int>(provider);

            List<int> resultList = myQuery.ToList();

            Assert.AreEqual(0, resultList.Count);
        }
    }
}
