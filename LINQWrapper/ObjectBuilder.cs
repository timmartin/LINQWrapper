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
    /// <summary>
    /// Maps a data reader object into an enumeration of proper .Net classes. Technically
    /// this ought to be called something like ObjectCollectionBuilder, since it builds
    /// more than one object. This does mapping with reflection and special attributes on the
    /// type, but ought not to have anything to do with the LINQ code per se.
    /// </summary>
    /// <typeparam name="T"></typeparam>
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

        /// <summary>
        /// Construct a single object of type T from a single row in the data reader.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
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

                Type targetType = property.PropertyType;

                if (targetType == typeof(int))
                {
                    int value = int.Parse(reader[fieldName].ToString());

                    property.SetValue(obj, value, null);
                }
                else if (targetType == typeof(string))
                {
                    string value = reader[fieldName].ToString();

                    property.SetValue(obj, value, null);
                }
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
