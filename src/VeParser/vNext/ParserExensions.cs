using System;
using System.Linq;

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
        public static Parser<TToken> If<TToken, TParsers>(this TParsers resultParsers, Func<TParsers, Parser<TToken>> combinator)
        {
            return new Parser<TToken>(input =>
                {
                    var parsers = resultParsers.AsDictionary<object>();
                    var resultsDictionary = (from p in parsers
                                             where !(p.Value is Parser<TToken>)
                                             select new { Name = p.Key, Value = p.Value })
                                            .ToDictionary(i => i.Name, i => i.Value);
                    var injectedParsersDictionary = (from p in parsers
                                                     where p.Value is Parser<TToken>
                                                     select new
                                                     {
                                                         Name = p.Key,
                                                         Value = new Parser<TToken>(pInput =>
                                                         {
                                                             var pOutout = ((Parser<TToken>)p.Value).Run(pInput);
                                                             resultsDictionary[p.Key] = pOutout.Result;
                                                             return pOutout;
                                                         })
                                                     }).ToDictionary(i => i.Name, i => i.Value);
                    resultParsers.FromDictionary(injectedParsersDictionary);

                    var output = combinator(resultParsers).Run(input);
                    if (output.Success)
                    {
                        return new ParseOutput<TToken>(output.Input, output.Position, true, resultsDictionary);
                    }
                    return output;
                });
        }
    }
}
