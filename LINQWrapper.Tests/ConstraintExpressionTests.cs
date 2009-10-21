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
    }
}
