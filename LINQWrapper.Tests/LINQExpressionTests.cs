using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using NMock2;
using NUnit.Framework;

using IQToolkit;
using LINQWrapper;
using LINQWrapper.Tests.TestTypes;

namespace LINQWrapper.Tests
{
    [TestFixture]
    class LINQExpressionTests
    {
        /// <summary>
        /// Check that we can execute an existing SQL expression within the LINQ wrapper, without
        /// worrying about modifying the expression via LINQ (yet)
        /// </summary>
        [Test]
        public void BasicExpressionTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();

            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();

            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT id FROM employees;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Exactly(4).On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(mockReader)
                .Get["id"]
                .Will(Return.Value("0"));

            Expect.Once.On(mockReader)
                .Get["name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(mockReader)
                .Get["id"]
                .Will(Return.Value("1"));

            Expect.Once.On(mockReader)
                .Get["name"]
                .Will(Return.Value("Bob"));

            Expect.Once.On(mockReader)
                .Get["id"]
                .Will(Return.Value("2"));

            Expect.Once.On(mockReader)
                .Get["name"]
                .Will(Return.Value("Charles"));

            Expect.Once.On(mockReader)
                .Get["id"]
                .Will(Return.Value("3"));

            Expect.Once.On(mockReader)
                .Get["name"]
                .Will(Return.Value("Dan"));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectClause("id");
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(mockConnection, builder);

            Query<Employee> myQuery = new Query<Employee>(provider);

            List<Employee> resultList = myQuery.ToList();

            Assert.AreEqual(4, resultList.Count);

            Assert.AreEqual(0, resultList[0].ID);
            Assert.AreEqual(1, resultList[1].ID);
            Assert.AreEqual(2, resultList[2].ID);
            Assert.AreEqual(3, resultList[3].ID);

            Assert.AreEqual("Alice", resultList[0].Name);
            Assert.AreEqual("Bob", resultList[1].Name);
            Assert.AreEqual("Charles", resultList[2].Name);
            Assert.AreEqual("Dan", resultList[3].Name);
        }
    }
}
