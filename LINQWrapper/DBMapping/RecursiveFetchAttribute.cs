using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBMapping
{
    /// <summary>
    /// Marks a member of class type as being worthy of recursively fetching
    /// in the ObjectBuilder.
    /// </summary>
    public class RecursiveFetchAttribute : Attribute
    {
        public RecursiveFetchAttribute()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeToConstruct">The type of object to construct, which may
        /// not necessarily be the same as the type of the property - it could be
        /// a subtype</param>
        public RecursiveFetchAttribute(Type typeToConstruct)
        {
            this.TypeToConstruct = typeToConstruct;
        }

        public Type TypeToConstruct
        { get; private set; }
    }
}
