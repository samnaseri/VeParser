using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    internal class ManagedParser<TToken> : Parser<TToken>
    {
        public ManagedParser()
            : base(null)
        {
        }
        internal virtual BranchCondition<TToken> GetBranchCondition()
        {
            return null;
        }
    }
}
