using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using NMock2;
using NUnit.Framework;

using LINQWrapper.Tests.TestTypes;

namespace LINQWrapper.Tests
{
    [TestFixture]
    public class ObjectBuilderTests
    {
        /// <summary>
        /// Test that we can instantiate a class with simple properties and no recursion
        /// </summary>
        [Test]
        public void SimpleInstantiation()
        {
            Mockery mocks = new Mockery();

            IDataReader reader = mocks.NewMock<IDataReader>();

            Expect.Once.On(reader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(reader)
                .Get["employee_id"]
                .Will(Return.Value(12));

            Expect.Once.On(reader)
                .Get["employee_name"]
                .Will(Return.Value("Alice"));

            Expect.Once.On(reader)
                .Method("Read")
                .Will(Return.Value(false));

            ObjectBuilder<Employee> builder = new ObjectBuilder<Employee>(reader);

            Employee alice = builder.Single();

            Assert.AreEqual(12, alice.ID);
            Assert.AreEqual("Alice", alice.Name);
        }

        /// <summary>
        /// Test that we can instantiate a class that has a class member that must be
        /// recursively instantiated and assigned to.
        /// </summary>
        [Test]
        public void RecursiveInstantiation()
        {
            Mockery mocks = new Mockery();

            IDataReader reader = mocks.NewMock<IDataReader>();

            Expect.Once.On(reader)
                .Method("Read")
                .Will(Return.Value(true));

            Expect.Once.On(reader)
                .Get["product_id"]
                .Will(Return.Value("12"));

            Expect.Once.On(reader)
                .Get["product_name"]
                .Will(Return.Value("Widgets"));

            Expect.Once.On(reader)
                .Get["supplier_id"]
                .Will(Return.Value("6"));

            Expect.Once.On(reader)
                .Get["supplier_name"]
                .Will(Return.Value("Wallace's Widgets & Co."));

            Expect.Once.On(reader)
                .Method("Read")
                .Will(Return.Value(false));

            ObjectBuilder<Product> products = new ObjectBuilder<Product>(reader);

            Product product = products.Single();

            Assert.AreEqual(12, product.ID);
            Assert.AreEqual("Widgets", product.Name);
            Assert.IsInstanceOf(typeof(Supplier), product.MainSupplier);
        }
    }
}
