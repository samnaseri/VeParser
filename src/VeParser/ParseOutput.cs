using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VeParser_vNext
{
    public class ParseOutput<TToken> : ParseState<TToken>
    {
        private bool success;
        public ParseOutput(IInput<TToken> sourceInput, int position, bool success, dynamic result = null)
            : base(sourceInput, position)
        {
            this.success = success;
            this.Result = result;
        }
        public bool Success
        {
            get
            {
                return success;
            }
        }
        public dynamic Result { get; private set; }
    }
}
