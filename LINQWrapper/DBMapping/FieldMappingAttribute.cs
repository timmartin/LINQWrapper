using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LINQWrapper.DBMapping
{
    /// <summary>
    /// A field mapping attribute describes how a property on a class is mapped to a field in the database.
    /// </summary>
    public class FieldMappingAttribute : Attribute
    {

        public FieldMappingAttribute(string fieldName, string uniqueFieldAlias)
        {
            this.FieldName = fieldName;
            this.UniqueFieldAlias = uniqueFieldAlias;
        }

        /// <summary>
        /// The single-parameter form of the constructor uses the field name as the globally-unique field
        /// alias - this is unlikely to be the right thing to do unless the field name is already very
        /// distinctive.
        /// </summary>
        /// <param name="fieldName"></param>
        public FieldMappingAttribute(string fieldName)
        {
            this.FieldName = fieldName;
            this.UniqueFieldAlias = fieldName;
        }

        public string FieldName
        { get; private set; }

        /// <summary>
        /// A name for the field that must be unique among all tables that this type could possibly be joined
        /// to. This will only be used within queries, so it need not be elegant. This defaults to being the
        /// same as the field name
        /// </summary>
        public string UniqueFieldAlias
        { get; private set; }
    }
}
