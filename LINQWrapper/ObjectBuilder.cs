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
        private static T MakeObject(IDataReader reader)
        {
            T obj = new T();

            Type objectType = typeof(T);

            var annotatedProperties = from property in objectType.GetProperties()
                                      where property.GetCustomAttributes(typeof(FieldMappingAttribute), false).Any()
                                      select property;

            foreach (PropertyInfo property in annotatedProperties)
            {
                string fieldName = ((FieldMappingAttribute)property.GetCustomAttributes(typeof(FieldMappingAttribute), false).First()).UniqueFieldAlias;

                Type targetType = property.PropertyType;

                object dbValue = reader[fieldName];

                if (!(dbValue is DBNull))
                {
                    if (targetType == typeof(int))
                    {
                        int value = int.Parse(dbValue.ToString());

                        property.SetValue(obj, value, null);
                    }
                    else if (targetType == typeof(string))
                    {
                        string value = dbValue.ToString();

                        property.SetValue(obj, value, null);
                    }
                    else if (targetType == typeof(DateTime))
                    {
                        DateTime value = DateTime.Parse(dbValue.ToString());

                        property.SetValue(obj, value, null);
                    }
                }
            }

            /* Now we find any properties of class type that we ought to instantiate
             * recursively */
            var propertiesToRecurse = from property in objectType.GetProperties()
                                      where property.GetCustomAttributes(typeof(RecursiveFetchAttribute), false).Any()
                                      select property;

            foreach (PropertyInfo property in propertiesToRecurse)
            {
                Type propertyType = property.PropertyType;

                RecursiveFetchAttribute attr = (RecursiveFetchAttribute)property.GetCustomAttributes(typeof(RecursiveFetchAttribute), false).Single();
                if (attr.TypeToConstruct != null)
                {
                    propertyType = attr.TypeToConstruct;
                }

                Type builderType = typeof(ObjectBuilder<>).MakeGenericType(new Type[] { propertyType });
                MethodInfo method = builderType.GetMethod("MakeObject", BindingFlags.Static | BindingFlags.NonPublic);
                object subObject = method.Invoke(null, new object[] { reader });

                property.SetValue(obj, subObject, null);
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
