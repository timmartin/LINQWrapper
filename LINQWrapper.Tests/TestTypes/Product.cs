using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper.Tests.TestTypes
{
    public class Product
    {
        [FieldMapping("id", "product_id")]
        public int ID
        { get; set; }

        [FieldMapping("name", "product_name")]
        public string Name
        { get; set; }

        [RecursiveFetch]
        public Supplier MainSupplier
        { get; set; }
    }
}
