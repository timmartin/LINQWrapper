using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LINQWrapper.SQLExpressions
{
    /// <summary>
    /// A row constraint, such as may appear in a SQL WHERE clause
    /// </summary>
    internal interface Constraint
    {
        /// <summary>
        /// Build this constraint into a string expression
        /// </summary>
        /// <param name="builder"></param>
        void BuildExpression(StringBuilder builder);

        /// <summary>
        /// Combine the current constraint with a given additional constraint and return the new
        /// constraint.
        /// </summary>
        /// <remarks>
        /// <para>This is useful because some constraints can combine more neatly than creating
        /// a new BooleanCombinationConstraint with two values in it. If we add boolean constraints
        /// one at a time we end up with one single boolean constraint combining them all, rather than
        /// a chain of nested constraints.</para>
        /// </remarks>
        /// <param name="other"></param>
        /// <param name="op"></param>
        /// <returns></returns>
        Constraint CombineConstraint(Constraint other, ExpressionType op);
    }
}
