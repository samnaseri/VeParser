using VeParser_vNext;

namespace VeParser
{
    public class C
    {
        public static Parser<char> Letter = (Parser<char>)(c => char.IsLetter(c));
        public static Parser<char> Digit = (Parser<char>)(c => char.IsDigit(c));
        public static Parser<char> WhiteSpace = (Parser<char>)(c => char.IsWhiteSpace(c));
        public static Parser<char> Upper = (Parser<char>)(c => char.IsUpper(c));
        public static Parser<char> Lower = (Parser<char>)(c => char.IsLower(c));
        public static Parser<char> LetterOrDigit = (Parser<char>)(c => char.IsLetterOrDigit(c));
        public static Parser<char> Number = (Parser<char>)(c => char.IsNumber(c));
        public static Parser<char> Punctuation = (Parser<char>)(c => char.IsPunctuation(c));
        public static Parser<char> Separator = (Parser<char>)(c => char.IsSeparator(c));
        public static Parser<char> Symbol = (Parser<char>)(c => char.IsSymbol(c));
        public static MultipleParserCombinator<char> Any = V.Any;
        public static MultipleParserCombinator<char> Seq = V.Seq;
        public static ParserCombinator<char> ZeroOrMore = V.ZeroOrMore;
        public static ParserCombinator<char> ZeroOrOne = V.ZeroOrOne;
        public static ParserCombinator<char> OneOrMore = V.OneOrMore;
        public static ParserCombinator<char> Not = V.Not;
    }
}
