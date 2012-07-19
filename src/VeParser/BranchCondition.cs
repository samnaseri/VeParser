using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    class BranchCondition<TToken>
    {
        public TToken[] ExpectedTokenSequence { get; set; }
    }
}
