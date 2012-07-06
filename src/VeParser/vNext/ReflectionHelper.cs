using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VeParser_vNext
{
    public static class ReflectionHelper
    {
        public static PropertyInfo[] GetProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }
        public static Dictionary<string, TType> AsDictionary<TType>(this object obj)
        {
            var outputValues = from property in GetProperties(obj) where typeof(TType).IsAssignableFrom(property.PropertyType) let PropertyName = property.Name let PropertyValue = (TType)property.GetValue(obj, new object[] { }) select new { PropertyName, PropertyValue };
            var result = new Dictionary<string, TType>();
            outputValues.ToList().ForEach(item => result.Add(item.PropertyName, item.PropertyValue));
            return result;
        }
        public static void FromDictionary<TType>(this object obj, Dictionary<string, TType> newValues)
        {
            var type = obj.GetType();
            foreach (var item in newValues)
            {
                var property = type.GetProperty(item.Key);
                property.SetValue(obj, item.Value, new object[] { });
            }
        }
    }
}
