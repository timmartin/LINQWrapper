using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;
using LINQWrapper.Tests.TestAttributes;

namespace LINQWrapper.Tests.TestTypes
{
    public class Supplier
    {
        [FieldMapping("id", "supplier_id")]
        public int ID
        { get; set; }

        [FieldMapping("name", "supplier_name")]
        [TestOrderByModifier]
        public string Name
        { get; set; }
    }
}
