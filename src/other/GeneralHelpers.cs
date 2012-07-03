using System;

namespace VeParser
{
    public static class outputHelper
    {
        public static string IndentText(this string text)
        {
            return text = " " + text.Replace("\n" , "\n ");
        }
    }

    public static class ObjectExtensions
    {
        public static void Feed(this object me , string fieldName , dynamic node)
        {
            if (!string.IsNullOrEmpty(fieldName)) {
                var field = me.GetType().GetField(fieldName);
                if (field.FieldType.Name.Contains("List")) {
                    dynamic list = field.GetValue(me);
                    if (list == null) {
                        list = Activator.CreateInstance(field.FieldType);
                        field.SetValue(me , list);
                    }
                    list.Add(node);
                }
                else if (field.FieldType == typeof(bool)) {
                    field.SetValue(me , true);
                }
                else if (field.FieldType.IsEnum) {
                    var value = System.Enum.Parse(field.FieldType , node , true);
                    field.SetValue(me , value);
                }
                else {
                    field.SetValue(me , node);
                }
            }
        }
    }
}