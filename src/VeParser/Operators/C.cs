using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    public static class C
    {
        public static Parser<char> Letter = (Parser<char>)char.IsLetter;
        public static Parser<char> Digit = (Parser<char>)char.IsDigit;
        public static Parser<char> WhiteSpace = (Parser<char>)char.IsWhiteSpace;
        public static Parser<char> Upper = (Parser<char>)char.IsUpper;
        public static Parser<char> Lower = (Parser<char>)char.IsLower;
        public static Parser<char> LetterOrDigit = (Parser<char>)char.IsLetterOrDigit;
        public static Parser<char> Number = (Parser<char>)char.IsNumber;
        public static Parser<char> Punctuation = (Parser<char>)char.IsPunctuation;
        public static Parser<char> Separator = (Parser<char>)char.IsSeparator;
        public static Parser<char> Symbol = (Parser<char>)char.IsSymbol;
        public static MultipleParserCombinator<char> Any = V.Any;
        public static MultipleParserCombinator<char> Seq = V.Seq;
        public static ParserCombinator<char> ZeroOrMore = V.ZeroOrMore;
        public static ParserCombinator<char> ZeroOrOne = V.ZeroOrOne;
        public static ParserCombinator<char> OneOrMore = V.OneOrMore;
        public static RepeatParserCombinator<char> Repeat = V.Repeat;
        public static ConditionalParserCombinator<char> Not = V.Not;
        public static Parser<char> Except(char ch)
        {
            return new Parser<char>((context, position) =>
            {
                var currentChar = context.Current(position);
                if (currentChar != ch)
                    return new ParseOutput<char>(position + 1, currentChar);
                else
                    return null;
            });
        }
        public static Parser<char> Except(params char[] characters)
        {
            var exemptCharacters = new System.Collections.Generic.SortedSet<char>(characters);
            return new Parser<char>((context, position) =>
            {
                var currentChar = context.Current(position);
                if (!exemptCharacters.Contains(currentChar))
                    return new ParseOutput<char>(position + 1, currentChar);
                else
                    return null;
            });
        }
        public static Parser<char> Range(char from, char to)
        {
            return new Parser<char>((context, position) =>
            {
                var currentChar = context.Current(position);
                if (currentChar >= from && currentChar <= to)
                    return new ParseOutput<char>(position + 1, currentChar);
                else
                    return null;
            });
        }

        public static Parser<char> CaptureText(Parser<char> parser)
        {
            return new Parser<char>((context, position) =>
            {
                var output = parser.Run(context, position);
                if (output != null)
                {
                    var text = GetText(output.Result);
                    return new ParseOutput<char>(output.Position, text);
                }
                return null;
            });
        }

        private static string GetText(object obj)
        {
            var charArray = obj as IEnumerable<char>;
            if (charArray != null)
                return new string(charArray.ToArray());
            var itemArray = obj as IEnumerable<object>;
            if (itemArray != null)
            {
                return string.Concat(itemArray.Select(i => GetText(i)));
            }
            if (obj is char)
            {
                return ((char)obj).ToString();
            }
            throw new Exception();
        }
    }
}
