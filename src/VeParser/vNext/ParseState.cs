using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VeParser_vNext
{
    public class ParseState<TToken>
    {
        private IInput<TToken> sourceInput;
        private int position;
        public ParseState(IInput<TToken> sourceInput, int position)
        {
            this.sourceInput = sourceInput;
            this.position = position;
        }
        public IInput<TToken> Input
        {
            get
            {
                return sourceInput;
            }
        }
        public int Position
        {
            get
            {
                return position;
            }
        }
        public TToken Current
        {
            get
            {
                return Input.GetTokenAtPosition(position);
            }
        }
    }
}
