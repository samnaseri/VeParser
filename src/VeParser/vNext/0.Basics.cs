// -----------------------------------------------------------------------
// <copyright file="0.Basics.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
namespace VeParser_vNext
{
    public interface IInput<TToken>
    {
        TToken GetTokenAtPosition(int position);
    }
    public class ParseState<TToken>
    {
        IInput<TToken> sourceInput;
        int position;

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
            get { return position; }
        }
        public TToken Current { get { return Input.GetTokenAtPosition(position); } }
    }
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
    public class ParseOutput<TToken> : ParseState<TToken>
    {
        bool success;
        public ParseOutput(IInput<TToken> sourceInput, int position, bool success)
            : base(sourceInput, position)
        {
            this.success = success;
        }
        public bool Success { get { return success; } }
    }
    public delegate ParseOutput<TToken> Parser<TToken>(ParseInput<TToken> input);

    public class V
    {
        private static Parser<TToken> ProceedIf<TToken>(bool condition)
        {
            return (input) =>
            {
                if (condition)
                    return new ParseOutput<TToken>(input.Input, input.Position + 1, true);
                else
                    return new ParseOutput<TToken>(input.Input, input.Position, false);
            };
        }
        private static Parser<TToken> ProceedIf<TToken>(Func<bool> condition)
        {
            return (input) =>
                {
                    var success = condition();
                    if (success)
                        return new ParseOutput<TToken>(input.Input, input.Position + 1, true);
                    else
                        return new ParseOutput<TToken>(input.Input, input.Position, false);
                };
        }
        private static Parser<TToken> ProceedIf<TToken>(Func<TToken, bool> condition)
        {
            return (input) =>
                {
                    if (condition(input.Current))
                        return new ParseOutput<TToken>(input.Input, input.Position + 1, true);
                    else
                        return new ParseOutput<TToken>(input.Input, input.Position, false);
                };
        }
        public static Parser<TToken> Token<TToken>(TToken expectedToken)
        {
            return ProceedIf<TToken>(currentToken => Comparer<TToken>.Default.Compare(currentToken, expectedToken) == 0);
        }
        public static Parser<TToken> EOI<TToken>()
        {
            return Token(default(TToken));
        }
        public static Parser<TToken> Seq<TToken>(params Parser<TToken>[] parsers)
        {
            return input =>
                {
                    var current = input;
                    foreach (var parser in parsers)
                    {
                        var output = parser(current);
                        if (!output.Success)
                            return new ParseOutput<TToken>(input.Input, input.Position, false);
                        current = new ParseInput<TToken>(output.Input, output.Position);
                    }
                    return new ParseOutput<TToken>(current.Input, current.Position, true);
                };
        }
        public static Parser<TToken> Any<TToken>(params Parser<TToken>[] parsers)
        {
            return input =>
                {
                    var current = input;
                    foreach (var parser in parsers)
                    {
                        var output = parser(input);
                        if (output.Success)
                            return new ParseOutput<TToken>(output.Input, output.Position, true);
                    }
                    return new ParseOutput<TToken>(input.Input, input.Position, false);
                };
        }
        public static Parser<TToken> ZeroOrMore<TToken>(Parser<TToken> parser)
        {
            return input =>
                {
                    var current = input;
                    while (true)
                    {
                        var output = parser(current);
                        if (!output.Success)
                            break;
                        current = new ParseInput<TToken>(output.Input, output.Position);
                        if (EOI<TToken>()(current).Success) // if reached to the end of input stream
                            break;
                    }
                    return new ParseOutput<TToken>(current.Input, current.Position, true);
                };
        }
        public static Parser<TToken> ZeroOrOne<TToken>(Parser<TToken> parser)
        {
            return input =>
                {
                    var output = parser(input);
                    if (output.Success)
                        return new ParseOutput<TToken>(output.Input, output.Position, true);
                    else
                        return new ParseOutput<TToken>(input.Input, input.Position, true);
                };
        }
        public static Parser<TToken> OneOrMore<TToken>(Parser<TToken> parser)
        {
            return input =>
                {
                    var current = input;
                    var output = parser(input);
                    if (!output.Success)
                        return new ParseOutput<TToken>(input.Input, input.Position, false);
                    else
                        current = new ParseInput<TToken>(output.Input, output.Position);

                    while (true)
                    {
                        output = parser(current);
                        if (!output.Success)
                            break;
                        current = new ParseInput<TToken>(output.Input, output.Position);
                        if (EOI<TToken>()(current).Success) // if reached to the end of input stream
                            break;
                    }
                    return new ParseOutput<TToken>(current.Input, current.Position, true);
                };
        }
    }
}
