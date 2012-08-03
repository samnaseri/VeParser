using System;
using VeParser.Predefined;

namespace VeParser.Test
{
    public class SimpleCSharpParser
    {
        public class Keyword
        {
            public string Word { get; set; }
        }
        public static void Parse(string code)
        {
            var characters = SpecialParsers.GetCodeCharactersParser();

            var csharp_keywords = new[] { "using", "namespace", "class", "private", "public", "protected", "internal", "event", "delegate", "var", "static" };

            Func<Func<char, bool>, Parser<CodeCharacter>> IF = condition => V.If<CodeCharacter>(c => condition(c.Character));

            var whitespace = V.If<CodeCharacter>(c => char.IsWhiteSpace(c.Character)).Star();
            whitespace = V.SkipWhile(whitespace);


            var QuoteScape = IF(c => c == '\\') + IF(c => c == '\"');
            var Quote = IF(c => c == '\"');
            var StringLiteral = V.Seq(Quote, (QuoteScape | Quote).Star());
            var BlockBegin = IF(c => c == '{');
            var BlockEnd = IF(c => c == '}');
        }
    }
}
