using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    internal class AnyParser<TToken> : ManagedParser<TToken>
    {
        public Parser<TToken>[] Parsers { get; set; }
        public AnyParser(Parser<TToken>[] parsers)
        {
            this.Parsers = parsers;
            CreateHandler();
        }
        private Parser<TToken>[] GetFlattenedParsers()
        {
            return Parsers.SelectMany(p =>
            {
                var seqParser = p as AnyParser<TToken>;
                if (seqParser != null)
                    return seqParser.GetFlattenedParsers();
                else
                    return new[] { p };
            }).ToArray();
        }
        internal void CreateHandler()
        {
            var parsersFlattened = GetFlattenedParsers();
            // Try to create a token/parser map to find the branch based on current token, instead of trial/error on each branch
            if (parsersFlattened.All(p => p is ManagedParser<TToken>))
            {
                // TODO : Should I only try to use a parser/token map only if there is enough Parsers(considering the memory cost of dictionary)
                var branchConditions = parsersFlattened.Cast<ManagedParser<TToken>>().Select(Parser => new { Parser, Tokens = Parser.GetBranchCondition() }).ToArray();
                if (branchConditions.All(c => c.Tokens != null))
                {
                    Dictionary<TToken, List<ParserHandler<TToken>>> parsersMap = new Dictionary<TToken, List<ParserHandler<TToken>>>();
                    foreach (var i in branchConditions)
                    {
                        var tokens = i.Tokens.ExpectedTokenSequence;
                        if (!parsersMap.ContainsKey(tokens[0]))
                            parsersMap[tokens[0]] = new List<ParserHandler<TToken>>();
                        parsersMap[tokens[0]].Add(i.Parser.parseHandler);
                    }
                    parseHandler = (context, position) =>
                    {
                        var currentToken = context.Current(position);
                        if (parsersMap.ContainsKey(currentToken))
                            foreach (var handler in parsersMap[currentToken])
                            {
                                var output = handler(context, position);
                                if (output != null)
                                    return output;
                            }
                        return null;
                    };
                    return;
                }
            }
            var handlers = parsersFlattened.Select(p => p.parseHandler).ToArray();
            parseHandler = (context, position) =>
            {
                foreach (var handler in handlers)
                {
                    var output = handler(context, position);
                    if (output != null)
                        return output;
                }
                return null;
            };
        }
        internal override BranchCondition<TToken> GetBranchCondition()
        {
            var parsersFlattened = GetFlattenedParsers();
            var branchesExpectations = parsersFlattened.TakeWhile(p => p is ManagedParser<TToken>).Cast<ManagedParser<TToken>>().Select(p => p.GetBranchCondition()).ToArray();
            if (branchesExpectations.Length > 0 && branchesExpectations.All(i => i != null))
            {
                var oneToken = branchesExpectations[0].ExpectedTokenSequence[0];
                if (branchesExpectations.Select(i => i.ExpectedTokenSequence.First()).All(i => Object.Equals(i, oneToken)))
                    return new BranchCondition<TToken> { ExpectedTokenSequence = new[] { oneToken } };
            }
            return null;
        }
    }
}
