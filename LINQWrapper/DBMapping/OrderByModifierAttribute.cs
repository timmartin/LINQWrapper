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
        /// <param name="fieldName"></param>
        /// <returns></returns>
        abstract public string GetOrderByExpression(string fieldName);
    }
}
