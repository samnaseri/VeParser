using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser_vNext
{
    public class Parser<TToken>
    {
        private ParserHandler<TToken> parseHandler;
        public Parser(ParserHandler<TToken> parseHandler)
        {
            this.parseHandler = parseHandler;
        }
        public ParseOutput<TToken> Run(ParseInput<TToken> input)
        {
            return parseHandler(input);
        }
        public static implicit operator Parser<TToken>(string value)
        {
            if (typeof(TToken) == typeof(char))
                return new Parser<TToken>(input =>
                {
                    List<char> nextTokens = new List<char>();
                    foreach (var p in Enumerable.Range(input.Position, value.Length))
                    {
                        var currentChar = input.Input.GetTokenAtPosition(p) as char?;
                        if (currentChar == null || currentChar != value[p - input.Position])
                            return V.ToFail(input);
                    }
                    return new ParseOutput<TToken>(input.Input, input.Position + value.Length, true, value);
                });

            throw new NotImplementedException("This method only works when TToken is char.");
        }
        public static implicit operator Parser<TToken>(Func<TToken, bool> tokenParser)
        {
            return V.ProceedIf(tokenParser, token => token);
        }
    }
}
