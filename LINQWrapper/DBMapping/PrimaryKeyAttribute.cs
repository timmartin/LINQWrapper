using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBMapping
{
    /// <summary>
    /// Marks the specified field as forming part of the primary key for the mapped table. This is
    /// used in COUNT(DISTINCT) expressions
    /// </summary>
    public class PrimaryKeyAttribute : Attribute
    {
    }
}
