using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBMapping
{
    public class FieldMappingAttribute : Attribute
    {
        public FieldMappingAttribute(string fieldName)
        {
            this.FieldName = fieldName;
        }

        public string FieldName
        { get; private set; }
    }
}
