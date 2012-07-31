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
    }
}
