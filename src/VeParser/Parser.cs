using System;

namespace VeParser
{
    /// <summary>
    /// Defines a parser object.
    /// </summary>
    /// <remarks>
    /// A parser object mainly is constituted of a handler function. The role of parser object is to hold extra information about that function.
    /// Such extra function could help other combinators to improve their algorithm.
    /// </remarks>
    /// <typeparam name="TToken">Specifies the type of stream being parsed.</typeparam>
    public class Parser<TToken>
    {
        /// <summary>
        /// The actual function responsible for handling the parse operation related to this parser instance.
        /// </summary>
        internal ParserHandler<TToken> parseHandler;

        /// <summary>
        /// Constructs a parser object receiving a handler function.
        /// </summary>
        /// <param name="parseHandler">The handler function which will perform the actual parsing process.</param>
        public Parser(ParserHandler<TToken> parseHandler)
        {
            this.parseHandler = parseHandler;
        }

        /// <summary>
        /// Invokes the parse handler on the given context and from the specified position.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public ParseOutput<TToken> Run(IParseContext<TToken> context, int position)
        {
            return parseHandler.Invoke(context, position);
            //return parseHandler(context, position);
        }

        /// <summary>
        /// Implicitly converts a string into a parser object. The resulted parser object works on char tokens.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
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
                for (int p = position, i = 0; i < chars.Length; p++, i++) {
                    var currentChar = context.Current(p);
                    if (currentChar != chars[i] || currentChar == default(char))
                        return null;
                }
                return new ParseOutput<char>(position + value.Length, value);
            });
        }
        /// <summary>
        /// Implicitly converts a acceptance function into a parser object.
        /// </summary>
        /// <param name="tokenParser">The acceptance parser.</param>
        /// <returns></returns>
        public static implicit operator Parser<TToken>(Func<TToken, bool> tokenParser)
        {
            return V.If<TToken>(tokenParser);
        }
        /// <summary>
        /// Implicitly converts a char into a parser object.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        public static implicit operator Parser<TToken>(char ch)
        {
            return (Parser<TToken>)(object)V.Token(ch);
        }
        /// <summary>
        /// Implicit operator for making * operator for repeat operator. Same as invoking <see cref="V.Repeat"/>.
        /// </summary>
        /// <remarks>
        /// When we need a pattern to be repeated an exact number of times we apply this operator.
        /// <code>
        /// <![CDATA[
        ///     Parser<char> ResultParser;
        ///     ResultParser = C.Digit * 6;
        /// ]]>
        /// </code>
        /// </remarks>
        /// <param name="parser">The parser which should be invoked repeatedly for the specified count.</param>
        /// <param name="repeatCount">The number of times the parser should be repeated, not less and not more.</param>
        /// <returns></returns>
        public static Parser<TToken> operator *(Parser<TToken> parser, int repeatCount)
        {
            return V.Repeat(parser, repeatCount, repeatCount);
        }
        /// <summary>
        /// Implicit operator for making + operator for seq operator. Same as invoking <see cref="V.Seq"/>.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Parser<TToken> operator +(Parser<TToken> p1, Parser<TToken> p2)
        {
            return V.Seq(p1, p2);
        }
        /// <summary>
        /// Implicit operator for making | for any operator. Same as invoking <see cref="V.Any"/>.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static Parser<TToken> operator |(Parser<TToken> p1, Parser<TToken> p2)
        {
            return V.Any(p1, p2);
        }
        public static Parser<TToken> operator >(Parser<TToken> p1, Parser<TToken> p2)
        {
            return V.TakeUntil(p2, p1);
        }
        public static Parser<TToken> operator <(Parser<TToken> p1, Parser<TToken> p2)
        {
            return V.TakeWhile(p1, p2);
        }
    }
}
