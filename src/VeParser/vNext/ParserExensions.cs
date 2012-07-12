using System;

namespace VeParser_vNext
{
    public static class ParserExensions
    {
        public static Parser<TToken> ProcessOutput<TToken, TNewResult>(this Parser<TToken> parser, Func<dynamic, TNewResult> renderFunc)
        {
            return new Parser<TToken>(input =>
            {
                var output = parser.Run(input);
                if (output.Success)
                {
                    var newResult = renderFunc(output.Result);
                    output = new ParseOutput<TToken>(output.Input, output.Position, output.Success, newResult);
                }
                return output;
            });
        }
        public static Parser<TToken> IgnoreOutput<TToken, TNewResult>(this Parser<TToken> parser)
        {
            return ProcessOutput(parser, output => default(TToken));
        }
    }
}
