using System;
using System.Collections;
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
        public static Parser<TToken> op_Implicit(string value)
        {
            if (typeof(TToken) == typeof(char))
                return V.Seq<TToken>(value.ToCharArray().Select(c => V.Token((TToken)((object)c))).ToArray());
            else
                throw new NotImplementedException("This method only works when TToken is char.");
        }
    }
}
