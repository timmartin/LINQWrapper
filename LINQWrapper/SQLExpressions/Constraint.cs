using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
