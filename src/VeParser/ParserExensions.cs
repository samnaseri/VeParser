using System;

namespace VeParser
{
    public static class ParserExensions
    {
        public static Parser<TToken> ProcessOutput<TToken, TNewResult>(this Parser<TToken> parser, Func<dynamic, TNewResult> renderFunc)
        {
            return new Parser<TToken>((context, position) =>
            {
                var output = parser.Run(context, position);
                if (output != null)
                {
                    var newResult = renderFunc(output.Result);
                    output = new ParseOutput<TToken>(output.Position, newResult);
                }
                return output;
            });
        }
        public static Parser<TToken> IgnoreOutput<TToken>(this Parser<TToken> parser)
        {
            return ProcessOutput(parser, output => default(TToken));
        }
    }
}
