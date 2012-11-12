using System;
using System.Collections.Generic;
using System.Globalization;
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
            return new Parser<char>((context, position) => {
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
            return new Parser<char>((context, position) => {
                var currentChar = context.Current(position);
                if (!exemptCharacters.Contains(currentChar))
                    return new ParseOutput<char>(position + 1, currentChar);
                else
                    return null;
            });
        }
        public static Parser<char> Range(char from, char to)
        {
            return new Parser<char>((context, position) => {
                var currentChar = context.Current(position);
                if (currentChar >= from && currentChar <= to)
                    return new ParseOutput<char>(position + 1, currentChar);
                else
                    return null;
            });
        }

        public static Parser<char> SkipWhitespace()
        {
            return V.SkipWhile(V.If<char>(c => char.IsWhiteSpace(c)));
        }

        private static string GetText(object obj)
        {
            var charArray = obj as IEnumerable<char>;
            if (charArray != null)
                return new string(charArray.ToArray());
            var itemArray = obj as IEnumerable<object>;
            if (itemArray != null) {
                return string.Concat(itemArray.Select(i => GetText(i)));
            }
            if (obj is char) {
                return ((char)obj).ToString();
            }
            throw new Exception();
        }
    }


    public class Set
    {
        public static Set Single(char ch) { return new Set(c => c == ch); }
        public static Set Range(char fromChar, char toChar) { return new Set(c => c >= fromChar && c <= toChar); }
        public static Set Printable = (Set)Set.Range(' ', (char)127) | (Set)Set.Single((char)160);
        public static Set CR = Set.Single('\r');
        public static Set LF = Set.Single('\n');
        public static Set Tab = Set.Single('\t');
        public static Set Whitespace = new Set(c => char.IsWhiteSpace(c));
        public static Set WhitespaceButNotNewLines = new Set(c => c != '\n' && c != '\r' && char.IsWhiteSpace(c));
        public static Set Unicode(UnicodeCategory category) { return new Set(c => char.GetUnicodeCategory(c) == category); }


        private Set(Func<char, bool> checkFunc) { this.checkFunc = checkFunc; }
        private Func<char, bool> checkFunc;

        public bool Check(char ch)
        {
            return this.checkFunc(ch);
        }

        public Set Add(char ch) { return null; }
        public Set Remove(char ch) { return null; }
        public Set Add(Set set) { return null; }
        public Set Remove(Set set) { return null; }

        public static Set operator |(Set set1, Set set2)
        {
            return new Set(c => set1.checkFunc(c) || set2.checkFunc(c));
        }
        public static Set operator -(Set set1, Set set2)
        {
            return new Set(c => set1.checkFunc(c) && !set2.checkFunc(c));
        }
        public static Parser<char> operator +(Set set1, Set set2)
        {
            return V.Seq(V.If(set1.checkFunc), V.If(set2.checkFunc));
        }
    }
}
