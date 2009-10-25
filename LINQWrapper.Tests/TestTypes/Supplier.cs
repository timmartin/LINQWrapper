using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper.Tests.TestTypes
{
    public class Supplier
    {
        [FieldMapping("supplier_id")]
        public int ID
        { get; set; }

        [FieldMapping("supplier_name")]
        public string Name
        { get; set; }
    }
}
