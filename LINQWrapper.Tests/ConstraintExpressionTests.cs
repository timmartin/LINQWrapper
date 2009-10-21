using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using NUnit.Framework;

using LINQWrapper.SQLExpressions;

namespace LINQWrapper.Tests
{
    [TestFixture]
    public class ConstraintExpressionTests
    {
        [Test]
        public void BooleanCombinationConstraint_BuildEmptyExpression()
        {
            BooleanCombinationConstraint constraint = new BooleanCombinationConstraint(ExpressionType.And);

            StringBuilder builder = new StringBuilder();

            constraint.BuildExpression(builder);

            Assert.AreEqual(" 1 ", builder.ToString());
        }

        [Test]
        public void AtomicConstraint_BuildExpression()
        {
            AtomicConstraint constraint = new AtomicConstraint("foo = bar");

            StringBuilder builder = new StringBuilder();

            constraint.BuildExpression(builder);

            Assert.AreEqual(" foo = bar ", builder.ToString());
        }

        [Test]
        public void BooleanCombinationConstraint_BuildAndExpression()
        {
            BooleanCombinationConstraint constraint = new BooleanCombinationConstraint(ExpressionType.And);

            constraint.AddConstraint(new AtomicConstraint("one=1"));
            constraint.AddConstraint(new AtomicConstraint("two=2"));
            constraint.AddConstraint(new AtomicConstraint("three=3"));

            StringBuilder builder = new StringBuilder();
            constraint.BuildExpression(builder);

            Assert.AreEqual("( one=1 ) AND ( two=2 ) AND ( three=3 )", builder.ToString());
        }

        /// <summary>
        /// When we use Boolean OR to combine a constraint into an existing OR expression, we get a single
        /// combined expression and not an additional layer of nesting
        /// </summary>
        [Test]
        public void BooleanCombinationConstraint_CombineConstraints()
        {
            BooleanCombinationConstraint baseConstraint = new BooleanCombinationConstraint(ExpressionType.Or);

            baseConstraint.AddConstraint(new AtomicConstraint("one=1"));
            baseConstraint.AddConstraint(new AtomicConstraint("two=2"));

            Constraint combinedConstraint = baseConstraint.CombineConstraint(new AtomicConstraint("three=3"), ExpressionType.Or);

            StringBuilder builder = new StringBuilder();

            combinedConstraint.BuildExpression(builder);

            Assert.AreEqual("( one=1 ) OR ( two=2 ) OR ( three=3 )", builder.ToString());
        }
    }
}
