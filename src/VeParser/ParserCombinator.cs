using System;

namespace VeParser
{
    public delegate Parser<TToken> ParserCombinator<TToken>(Parser<TToken> source);
    public delegate Parser<TToken> MultipleParserCombinator<TToken>(params Parser<TToken>[] parsers);
    public delegate Parser<TToken> RepeatParserCombinator<TToken>(Parser<TToken> parser, int? minRepeatCount, int? maxRepeatCount);
    public delegate Parser<TToken> ConditionalParserCombinator<TToken>(Func<TToken, bool> condition);
}
