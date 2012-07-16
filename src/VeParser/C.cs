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
    }
}
