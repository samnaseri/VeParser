namespace VeParser
{
    /// <summary>
    /// Defines a shared context which be accessible to all parsers running throughout a parsing process.
    /// </summary>
    /// <typeparam name="TToken"></typeparam>
    public interface IParseContext<TToken>
    {
        TToken Current(int position);
    }

    public class SimpleParseContext<TToken> : IParseContext<TToken>
    {
        internal readonly IInput<TToken> tokens;
        /// <summary>
        /// The constructor of the context.
        /// </summary>
        /// <param name="tokens">The list of tokens which should be parsed.</param>
        public SimpleParseContext(IInput<TToken> tokens)
        {
            this.tokens = tokens;
        }

        public TToken Current(int position)
        {
            return tokens.GetTokenAtPosition(position);
        }
    }
    public class WindowedParseContext<TToken> : IParseContext<TToken>
    {
        IParseContext<TToken> source;
        int fromPosition;
        int toPosition;

        public WindowedParseContext(IParseContext<TToken> source, int fromPosition, int toPosition)
        {
            this.source = source;
            this.fromPosition = fromPosition;
            this.toPosition = toPosition;
        }
        public TToken Current(int position)
        {
            if ( position >= fromPosition && position <= toPosition)
                return source.Current(position);
            else
                return default(TToken);
        }
    }
}
