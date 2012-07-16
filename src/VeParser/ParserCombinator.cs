using VeParser_vNext;

namespace VeParser
{
    public delegate Parser<TToken> ParserCombinator<TToken>(Parser<TToken> source);
    public delegate Parser<TToken> MultipleParserCombinator<TToken>(params Parser<TToken>[] parsers);
}
