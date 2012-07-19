using System;
using System.Linq;

namespace VeParser
{
    internal class SeqParser<TToken> : ManagedParser<TToken>
    {
        public Parser<TToken>[] Parsers { get; set; }
        public SeqParser(Parser<TToken>[] parsers)
        {
            this.Parsers = parsers;
            CreateHandler();
        }
        private Parser<TToken>[] GetFlattenedParsers()
        {
            return Parsers.SelectMany(p =>
            {
                var seqParser = p as SeqParser<TToken>;
                if (seqParser != null)
                    return seqParser.GetFlattenedParsers();
                else
                    return new[] { p };
            }).ToArray();
        }
        internal void CreateHandler()
        {
            var parsersFlattened = GetFlattenedParsers();
            var handlers = parsersFlattened.Select(p => p.parseHandler).ToArray();
            if (parsersFlattened.All(p => p is TokenParser<TToken>))
            {
                var tokens = parsersFlattened.Cast<TokenParser<TToken>>().Select(p => p.Expected).ToArray();
                parseHandler = (context, position) =>
                {
                    for (ushort p = position; p < position + tokens.Length; p++)
                    {
                        var currentToken = context.tokens.GetTokenAtPosition(p);
                        if (!Object.Equals(currentToken, tokens[p - position]) || Object.Equals(currentToken, default(TToken)))
                            return null;
                    }
                    return new ParseOutput<TToken>((ushort)(position + tokens.Length), tokens);
                };
            }
            else
                parseHandler = (context, position) =>
                {
                    var currentPosition = position;
                    var results = new object[parsersFlattened.Length];
                    for (int index = 0; index < parsersFlattened.Length; index++)
                    {
                        var output = handlers[index](context, currentPosition);
                        if (output == null)
                            return null;
                        results[index] = output.Result;
                        currentPosition = output.Position;
                    }
                    return new ParseOutput<TToken>(currentPosition, results);
                };
        }
        internal override BranchCondition<TToken> GetBranchCondition()
        {
            var parsersFlattened = GetFlattenedParsers();
            var tokensExpectedInSequnce = parsersFlattened.TakeWhile(p => p is ManagedParser<TToken>).Cast<ManagedParser<TToken>>().Select(p => p.GetBranchCondition()).TakeWhile(i => i != null).SelectMany(i => i.ExpectedTokenSequence).ToArray();
            if (tokensExpectedInSequnce.Length > 0)
                return new BranchCondition<TToken> { ExpectedTokenSequence = tokensExpectedInSequnce };
            else
                return null;
        }
    }
}
