using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper.Tests.TestTypes
{
    public class Product
    {
        [FieldMapping("product_id")]
        public int ID
        { get; set; }

        [FieldMapping("product_name")]
        public string Name
        { get; set; }

        [RecursiveFetch]
        public Supplier MainSupplier
        { get; set; }
    }
}
