using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.SQLExpressions
{
    /// <summary>
    /// A row constraint that consists of a single expression that is (from our point of view)
    /// atomic. This means that it doesn't have any boolean operators within it that we can
    /// act on, although it may have all sorts of other operators within it that we choose
    /// not to work with.
    /// </summary>
    internal class AtomicConstraint : Constraint
    {
        public AtomicConstraint(string constraint)
        {
            this.constraint = constraint;
        }

        #region Constraint Members

        public void BuildExpression(StringBuilder builder)
        {
            builder.Append(" ");
            builder.Append(constraint);
            builder.Append(" ");
        }

        public Constraint CombineConstraint(Constraint other, System.Linq.Expressions.ExpressionType op)
        {
            BooleanCombinationConstraint combinedConstraint = new BooleanCombinationConstraint(op);

            combinedConstraint.AddConstraint(this);
            combinedConstraint.AddConstraint(other);

            return combinedConstraint;
        }

        #endregion

        #region Private data members

        private string constraint;

        #endregion
    }
}
