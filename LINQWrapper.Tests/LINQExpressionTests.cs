using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
                .SetProperty("CommandText").To("SELECT DISTINCT id FROM employees;");

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

            Expect.Once.On(mockReader)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            // NB: The select clause we're setting here isn't sufficient to instantiate the objects,
            // which have two fields. This doesn't matter here since we are using mock objects
            // anyway, and in practice it will be up to the author of the SQL statements to get details
            // like this right.
            builder.AddSelectClause("id");
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(mockConnection, builder, new Dictionary<string, object>());

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

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        /// <summary>
        /// Check that when we call the OrderBy() method on an already-constructed query, it 
        /// causes different SQL to be produced
        /// </summary>
        [Test]
        public void LazyOrderingTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT id, name FROM employees ORDER BY name;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            // We return an empty result set from the mock reader. This doesn't matter much, since we
            // verify that the ORDER BY is set in the SQL query executed.

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectClause("id");
            builder.AddSelectClause("name");
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(mockConnection, builder, new Dictionary<string, object>());
            Query<Employee> myQuery = new Query<Employee>(provider);

            var orderedResults = from x in myQuery
                                 orderby x.Name
                                 select x;

            List<Employee> resultsList = orderedResults.ToList();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        /// <summary>
        /// Check that we can apply a LINQ Count() expression and have it turned into SQL
        /// </summary>
        [Test]
        public void CountExpressionTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT COUNT(*) AS numrows FROM employees WHERE  name LIKE '%smith' ;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            // Note that we only call the reader once, rather than iterating through until
            // it returns false. This works fine when we know we have only one result,
            // which will be the case until we implement GroupBy
            /*Expect.Once.On(mockReader)
                  .Method("Read")
                  .Will(Return.Value(false));*/

            Expect.Once.On(mockReader)
                .Get["numrows"]
                .Will(Return.Value(16));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectClause("id");
            builder.AddSelectClause("name");
            builder.AddFromClause("employees");
            builder.AddWhereClause("name LIKE '%smith'", ExpressionType.And);

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(mockConnection, builder, new Dictionary<string, object>());
            Query<Employee> myQuery = new Query<Employee>(provider);

            Assert.AreEqual(16, myQuery.Count());

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        [Test]
        public void SetParametersTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();
            IDbDataParameter mockParameter = mocks.NewMock<IDbDataParameter>();
            IDataParameterCollection mockParameterCollection = mocks.NewMock<IDataParameterCollection>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT id, name FROM employees WHERE  name=@name ;");

            Expect.Once.On(mockCommand)
                .Method("CreateParameter")
                .Will(Return.Value(mockParameter));

            Expect.Once.On(mockParameter)
                .SetProperty("ParameterName").To("@name");

            Expect.Once.On(mockParameter)
                .SetProperty("Value").To("smith");

            Expect.Once.On(mockCommand)
                .GetProperty("Parameters")
                .Will(Return.Value(mockParameterCollection));

            Expect.Once.On(mockParameterCollection)
                .Method("Add").With(mockParameter)
                .Will(Return.Value(0));

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            // We don't return any values, since we don't need to test what we do with
            // the returned values. The important thing to test is that the parameter
            // methods above get called.
            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id");
            sqlBuilder.AddSelectClause("name");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.AddWhereClause("name=@name", ExpressionType.And);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["@name"] = "smith";

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(mockConnection, sqlBuilder, parameters);
            Query<Employee> query = new Query<Employee>(provider);

            List<Employee> employees = query.ToList();

            // The important testing is done in the mock code. Provided the right 
            // AddParameter stuff is called, we don't care what happens with the results

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}
