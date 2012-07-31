using System;

namespace VeParser
{
    internal class TokenParser<TToken> : ManagedParser<TToken>
    {
        public TToken Expected { get; set; }
        public TokenParser(TToken expected)
        {
            this.Expected = expected;
            CreateHandler();
        }
        internal void CreateHandler()
        {
            parseHandler = (context, position) => {
                var current = context.Current(position);
                if (Object.Equals(Expected, current))
                    return new ParseOutput<TToken>(position + 1, current);
                else
                    return null;
            };
        }
    }
}
