using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

using LINQWrapper.DBMapping;

namespace LINQWrapper
{
    internal class ObjectBuilder<T> : IEnumerable<T>, IEnumerable where T : class, new()
    {
        public ObjectBuilder(IDataReader reader)
        {
            results = new List<T>();

            while (reader.Read())
            {
                T obj = MakeObject(reader);
                results.Add(obj);
            }
        }

        private T MakeObject(IDataReader reader)
        {
            T obj = new T();

            Type objectType = typeof(T);

            var annotatedProperties = from property in objectType.GetProperties()
                                      where property.GetCustomAttributes(typeof(FieldMappingAttribute), false).Any()
                                      select property;

            foreach (PropertyInfo property in annotatedProperties)
            {
                string fieldName = ((FieldMappingAttribute)property.GetCustomAttributes(typeof(FieldMappingAttribute), false).First()).FieldName;

                string value = reader[fieldName].ToString();
            }

            return obj;
        }

        private List<T> results;

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return results.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return results.GetEnumerator();
        }

        #endregion
    }
}
