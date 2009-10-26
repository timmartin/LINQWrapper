using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper.Tests.TestTypes
{
    internal class Employee : IPerson
    {
        [FieldMapping("id", "employee_id")]
        public int ID
        {
            get;
            set; 
        }

        [FieldMapping("name", "employee_name")]
        public string Name
        {
            get;
            set;
        }
    }
}
