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
            parseHandler = (context, position) =>
            {
                var current = context.Current(position);
                if (Object.Equals(Expected, current))
                    return new ParseOutput<TToken>((ushort)(position + 1), current);
                else
                    return null;
            };
        }
        internal override BranchCondition<TToken> GetBranchCondition()
        {
            return new BranchCondition<TToken> { ExpectedTokenSequence = new TToken[] { Expected } };
        }
    }
}
