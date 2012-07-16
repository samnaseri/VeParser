using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VeParser_vNext
{
    public delegate ParseOutput<TToken> ParserHandler<TToken>(ParseInput<TToken> input);
}
