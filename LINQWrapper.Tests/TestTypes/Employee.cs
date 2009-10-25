using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper.Tests.TestTypes
{
    internal class Employee : IPerson
    {
        [FieldMapping("id")]
        public int ID
        {
            get;
            set; 
        }

        [FieldMapping("name")]
        public string Name
        {
            get;
            set;
        }
    }
}
