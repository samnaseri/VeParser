using System;

namespace VeParser
{
    public class Parser<TToken>
    {
        internal ParserHandler<TToken> parseHandler;
        public Parser(ParserHandler<TToken> parseHandler)
        {
            this.parseHandler = parseHandler;
        }
        public ParseOutput<TToken> Run(IParseContext<TToken> context, int position)
        {
            return parseHandler(context, position);
        }
        public static implicit operator Parser<TToken>(string value)
        {
            if (typeof(TToken) == typeof(char)) {
                return (Parser<TToken>)(object)StringToParser(value);
            }
            return V.If<TToken>(token => token.ToString() == value);
        }
        private static Parser<char> StringToParser(string value)
        {
            var chars = value.ToCharArray();
            return new Parser<char>((context, position) => {
                for (var p = position; p < position + value.Length; p++) {
                    var currentChar = context.Current(p);
                    if (currentChar != chars[p - position] || currentChar == default(char))
                        return null;
                }
                return new ParseOutput<char>(position + value.Length, value);
            });
        }
        public static implicit operator Parser<TToken>(Func<TToken, bool> tokenParser)
        {
            return V.If<TToken>(tokenParser);
        }
        public static implicit operator Parser<TToken>(char ch)
        {
            return (Parser<TToken>)(object)V.Token(ch);
        }


        public static Parser<TToken> operator *(Parser<TToken> p1, int number)
        {
            return V.Repeat(p1, number, number);
        }
        public static Parser<TToken> operator +(Parser<TToken> p1, Parser<TToken> p2)
        {
            return V.Seq(p1, p2);
        }
        public static Parser<TToken> operator |(Parser<TToken> p1, Parser<TToken> p2)
        {
            return V.Any(p1, p2);
        }
    }
}
