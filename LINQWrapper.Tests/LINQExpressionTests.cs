using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
                .Get["employee_id"]
                .Will(Return.Value("0"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("1"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Bob"));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("2"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Charles"));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("3"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Dan"));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            // NB: The select clause we're setting here isn't sufficient to instantiate the objects,
            // which have two fields. This doesn't matter here since we are using mock objects
            // anyway, and in practice it will be up to the author of the SQL statements to get details
            // like this right.
            builder.AddSelectClause("id");
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, builder, new Dictionary<string, object>());

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
                .SetProperty("CommandText").To("SELECT DISTINCT employees.id AS employee_id, employees.name AS employee_name FROM employees ORDER BY employee_name;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            // We return an empty result set from the mock reader. This doesn't matter much, since we
            // verify that the ORDER BY is set in the SQL query executed.

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectTypeClause("employees", typeof(Employee));
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, builder, new Dictionary<string, object>());
            Query<Employee> myQuery = new Query<Employee>(provider);

            var orderedResults = from x in myQuery
                                 orderby x.Name
                                 select x;

            List<Employee> resultsList = orderedResults.ToList();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        /// <summary>
        /// Check that we can do an orderby foo.Bar.Baz with a member of a member
        /// </summary>
        [Test]
        public void OrderBySubMemberTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT products.id AS product_id, products.name AS product_name, suppliers.id AS supplier_id, suppliers.name AS supplier_name FROM products JOIN suppliers ON products.main_supplier = suppliers.id ORDER BY supplier_name;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectTypeClause("products", typeof(Product));
            builder.AddSelectTypeClause("suppliers", typeof(Supplier));

            builder.AddFromClause("products");
            builder.AddJoinClause("JOIN", "suppliers", "products.main_supplier = suppliers.id");

            LazyDBQueryProvider<Product> provider = new LazyDBQueryProvider<Product>(() => mockConnection, builder, new Dictionary<string, object>());
            Query<Product> myQuery = new Query<Product>(provider);

            var orderedResults = from x in myQuery
                                 orderby x.MainSupplier.Name
                                 select x;

            List<Product> resultsList = orderedResults.ToList();

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

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            builder.AddSelectClause("id");
            builder.AddSelectClause("name");
            builder.AddFromClause("employees");
            builder.AddWhereClause("name LIKE '%smith'", ExpressionType.And);

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, builder, new Dictionary<string, object>());
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

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id");
            sqlBuilder.AddSelectClause("name");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.AddWhereClause("name=@name", ExpressionType.And);

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            parameters["@name"] = "smith";

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, sqlBuilder, parameters);
            Query<Employee> query = new Query<Employee>(provider);

            List<Employee> employees = query.ToList();

            // The important testing is done in the mock code. Provided the right 
            // AddParameter stuff is called, we don't care what happens with the results

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        /// <summary>
        /// Check that we can apply a LINQ Cast&lt;IPerson&gt;() operator and get appropriate
        /// behaviour.
        /// </summary>
        [Test]
        public void CastExpressionTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT id AS employee_id, name AS employee_name FROM employees;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("0"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");
            
            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id AS employee_id");
            sqlBuilder.AddSelectClause("name AS employee_name");
            sqlBuilder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, sqlBuilder, new Dictionary<string, object>());
            Query<Employee> query = new Query<Employee>(provider);

            IQueryable<IPerson> people = query.Cast<IPerson>();

            List<IPerson> peopleList = people.ToList();

            Assert.AreEqual(1, peopleList.Count);
            Assert.IsInstanceOf<IPerson>(peopleList[0]); // This can't actually fail, but it's nice to record our intention
        }

        /// <summary>
        /// Check that if we run the same query twice with slight modifications, it doesn't end up
        /// wrecking the internal state. Note that we're not checking caching of results here, which 
        /// is a separate issue.
        /// </summary>
        [Test]
        public void QueryRepeatabilityTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand firstMockCommand = mocks.NewMock<IDbCommand>();
            IDbCommand secondMockCommand = mocks.NewMock<IDbCommand>();
            IDataReader firstMockReader = mocks.NewMock<IDataReader>();
            IDataReader secondMockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(firstMockCommand));

            Expect.Once.On(firstMockCommand)
                .SetProperty("CommandText").To("SELECT COUNT(*) AS numrows FROM employees WHERE  id=42 ;");

            Expect.Once.On(firstMockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(firstMockReader));

            Expect.Once.On(firstMockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(firstMockReader)
                .Get["numrows"]
                .Will(Return.Value(1));

            Expect.Once.On(firstMockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(secondMockCommand));

            Expect.Once.On(secondMockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT id AS employee_id, name AS employee_name FROM employees WHERE  id=42 ;");

            Expect.Once.On(secondMockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(secondMockReader));

            Expect.Once.On(secondMockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(secondMockReader)
                .Get["employee_id"]
                .Will(Return.Value(1));

            Expect.Once.On(secondMockReader)
                .Get["employee_name"]
                .Will(Return.Value("Bob"));

            Expect.Once.On(secondMockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(secondMockReader)
                .Method("Dispose");

            // NB: We're returning the same connection object twice, so we'll dispose it twice. This
            // isn't really correct, but since these are mock objects nobody will know that they're
            // the same object 
            Expect.Exactly(2).On(mockConnection)
                .Method("Dispose");

            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id AS employee_id");
            sqlBuilder.AddSelectClause("name AS employee_name");
            sqlBuilder.AddFromClause("employees");
            sqlBuilder.AddWhereClause("id=42", ExpressionType.And);

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, sqlBuilder, new Dictionary<string, object>());

            Query<Employee> query = new Query<Employee>(provider);

            int numEmployees = query.Count();
            Assert.AreEqual(1, numEmployees);

            List<Employee> employees = query.ToList();
        }

        /// <summary>
        /// Check that we can do expressions with Skip() and Take() in them
        /// </summary>
        [Test]
        public void SkipTakeTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT id AS employee_id, name AS employee_name FROM employees LIMIT 10, 15;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("0"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectClause("id AS employee_id");
            sqlBuilder.AddSelectClause("name AS employee_name");
            sqlBuilder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, sqlBuilder, new Dictionary<string, object>());
            Query<Employee> query = new Query<Employee>(provider);

            List<Employee> employeeList = query.Skip(10).Take(15).ToList();

            Assert.AreEqual(1, employeeList.Count); // Though we asked for 15 results, there was only one returned
            Assert.AreEqual(0, employeeList[0].ID);
            Assert.AreEqual("Alice", employeeList[0].Name);
        }

        /// <summary>
        /// We had a bug when combining Take() with a Cast() expression
        /// </summary>
        [Test]
        public void TakeAndCastTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT DISTINCT employees.id AS employee_id, employees.name AS employee_name FROM employees LIMIT 10;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectTypeClause("employees", typeof(Employee));
            sqlBuilder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, sqlBuilder, new Dictionary<string, object>());
            Query<Employee> query = new Query<Employee>(provider);

            List<IPerson> employeeList = query.Take(10).Cast<IPerson>().ToList();

            // We've already checked that the correct SQL is sent, so no need to check anything
            // further here
        }

        /// <summary>
        /// Check that if we execute the same query twice with the same expression, the query isn't
        /// run twice.
        /// </summary>
        [Test]
        public void QueryCachingTest()
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

            Expect.Exactly(1).On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("0"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            // NB: The select clause we're setting here isn't sufficient to instantiate the objects,
            // which have two fields. This doesn't matter here since we are using mock objects
            // anyway, and in practice it will be up to the author of the SQL statements to get details
            // like this right.
            builder.AddSelectClause("id");
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, builder, new Dictionary<string, object>());

            Query<Employee> myQuery = new Query<Employee>(provider);

            List<Employee> resultList = myQuery.ToList();

            Assert.AreEqual(1, resultList.Count);
            Assert.AreEqual(0, resultList[0].ID);
            Assert.AreEqual("Alice", resultList[0].Name);

            // This shouldn't cause any further DB queries, since we've already evaluated this
            // expression once
            resultList = myQuery.ToList();

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        /// <summary>
        /// Check that we can fetch results from a query with the ElementAt() operator
        /// </summary>
        [Test]
        public void ElementAtTest()
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

            Expect.Exactly(2).On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("0"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(mockReader)
                .Get["employee_id"]
                .Will(Return.Value("1"));

            Expect.Once.On(mockReader)
                .Get["employee_name"]
                .Will(Return.Value("Bob"));

            Expect.Once.On(mockReader)
                .Method("Read")
                .Will(Return.Value(false));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder builder = new MySQLBuilder();

            // NB: The select clause we're setting here isn't sufficient to instantiate the objects,
            // which have two fields. This doesn't matter here since we are using mock objects
            // anyway, and in practice it will be up to the author of the SQL statements to get details
            // like this right.
            builder.AddSelectClause("id");
            builder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, builder, new Dictionary<string, object>());

            Query<Employee> myQuery = new Query<Employee>(provider);

            Assert.AreEqual(0, myQuery.ElementAt(0).ID);
            Assert.AreEqual("Alice", myQuery.ElementAt(0).Name);
            Assert.AreEqual(1, myQuery.ElementAt(1).ID);
            Assert.AreEqual("Bob", myQuery.ElementAt(1).Name);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }

        /// <summary>
        /// We should be able to override the SQL for particular optimisation cases by 
        /// providing alternative SQL
        /// </summary>
        [Test]
        public void ExpressionOptimisationTest()
        {
            Mockery mocks = new Mockery();

            IDbConnection mockConnection = mocks.NewMock<IDbConnection>();
            IDbCommand mockCommand = mocks.NewMock<IDbCommand>();
            IDataReader mockReader = mocks.NewMock<IDataReader>();

            Expect.Once.On(mockConnection)
                .Method("CreateCommand")
                .Will(Return.Value(mockCommand));

            Expect.Once.On(mockCommand)
                .SetProperty("CommandText").To("SELECT stat FROM pre_calculated_count;");

            Expect.Once.On(mockCommand)
                .Method("ExecuteReader")
                .Will(Return.Value(mockReader));

            Expect.Exactly(1).On(mockReader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(mockReader)
                .Get["numrows"]
                .Will(Return.Value("42"));

            Expect.Once.On(mockReader)
                .Method("Dispose");

            Expect.Once.On(mockConnection)
                .Method("Dispose");

            SQLBuilder sqlBuilder = new MySQLBuilder();

            sqlBuilder.AddSelectTypeClause("employees", typeof(Employee));
            sqlBuilder.AddFromClause("employees");

            LazyDBQueryProvider<Employee> provider = new LazyDBQueryProvider<Employee>(() => mockConnection, sqlBuilder, new Dictionary<string, object>());
            Query<Employee> query = new Query<Employee>(provider);

            Expression<Func<int>> countExpr = () => query.Count();

            Assert.NotNull(countExpr);

            provider.SetOptimisedCountExpression("SELECT stat FROM pre_calculated_count;");

            int count = query.Count();

            Assert.AreEqual(42, count);

            mocks.VerifyAllExpectationsHaveBeenMet();
        }
    }
}
