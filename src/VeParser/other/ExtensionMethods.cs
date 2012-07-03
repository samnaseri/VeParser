
namespace VeParser
{
    public static class ExtensionMethods
    {
        public static string IndentText(this string text)
        {
            return text = " " + text.Replace("\n", "\n ");
        }
    }
}