using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VeParser_vNext
{
    public class ParseInput<TToken> : ParseState<TToken>
    {
        public ParseInput(IInput<TToken> sourceInput, int position)
            : base(sourceInput, position)
        {
        }
        public ParseInput(IInput<TToken> sourceInput)
            : this(sourceInput, 0)
        {
        }
    }
}
