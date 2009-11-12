using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBMapping
{
    /// <summary>
    /// If the field being mapped wants a different expression in the ORDER BY
    /// field than just the name of the field, apply an attribute that implements
    /// this interface.
    /// </summary>
    /// <remarks>
    /// This is an abstract class rather than an interface because we must extend
    /// Attribute, which is itself a class
    /// </remarks>
    public abstract class OrderByModifierAttribute : Attribute
    {
        /// <summary>
        /// This should return an expression, based on the field name, that will be
        /// included in the SQL ORDER BY expression in place of the field itself.
        /// </summary>
        /// <remarks>
        /// This method must build in the sort direction itself, since it isn't possible in general
        /// for the abstraction layer to decide where to put the 'DESC' keyword if it doesn't know
        /// how the expression has been transformed
        /// </remarks>
        /// <param name="fieldName">The name of the field to sort by, as it would be included
        /// in the ORDER BY column if the modifier wasn't there. This will be the unique field
        /// alias of the field being selected</param>
        /// <param name="direction">The direction in which to sort</param>
        /// <returns></returns>
        abstract public string GetOrderByExpression(string fieldName, SortDirection direction);
    }
}
