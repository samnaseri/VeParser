namespace VeParser
{
    /// <summary>
    /// Defines a shared context which be accessible to all parsers running throughout a parsing process.
    /// </summary>
    /// <typeparam name="TToken"></typeparam>
    public class ParseContext<TToken>
    {
        internal readonly IInput<TToken> tokens;
        /// <summary>
        /// The constructor of the context.
        /// </summary>
        /// <param name="tokens">The list of tokens which should be parsed.</param>
        public ParseContext(IInput<TToken> tokens)
        {
            this.tokens = tokens;
        }

        public TToken Current(ushort position)
        {
            return tokens.GetTokenAtPosition(position);
        }
    }
}
