
namespace VeParser.Test
{
    public class Parrot
    {
        Set StringChars = Set.Printable | Set.Tab - Set.Single('"');
        Set MultiLineStringChars = Set.Printable | Set.Tab | Set.CR | Set.LF;
        public static Parser<char> getParser()
        {
            Parser<char> Identifier = Set.Range('a', 'z') + Set.Range('0', '9');
            return Identifier;
        }
    }
}
