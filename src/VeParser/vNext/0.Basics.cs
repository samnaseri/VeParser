﻿// -----------------------------------------------------------------------
// <copyright file="0.Basics.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser_vNext
{
    /// <summary>
    /// Interface defining a input source stream for VeParser.
    /// </summary>
    /// <typeparam name="TToken">
    /// The type of tokens in the input stream. If you are writing a tokenizer then this usually TToken would be <see cref="char"/>, or it 
    /// may be string or your custom Token class if you are consuming a list of tokens.
    /// </typeparam>
    public interface IInput<TToken>
    {
        /// <summary>
        /// When implemented should return a token at specified point.
        /// </summary>
        /// <remarks>
        /// position will start from zero and will increase one by one. However the parser may revert to an earlier position. For example when <see cref="V.Any"/> is used then
        /// different alternative parsing grammars will be checked so, it will repeatedly start from the position in which any started, until a grammar succeed.
        /// <example>
        /// Consider implementing this interface for a string value as source stream. The the implementation will basically will use string indexer to return the char
        /// value at the desired position.
        /// </example>
        /// </remarks>
        /// <param name="position">The desirec position.</param>
        /// <returns>The token at the desired position.</returns>
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
#if dotnet2
        public ParseOutput(IInput<TToken> sourceInput, int position, bool success, object result = null)
#else
        public ParseOutput(IInput<TToken> sourceInput, int position, bool success, dynamic result = null)
#endif
            : base(sourceInput, position)
        {
            this.success = success;
            this.Result = result;
        }
        public bool Success { get { return success; } }

#if dotnet2
        public object Result { get; private set;}
#else
        public dynamic Result { get; private set; }
#endif

    }
    public class Parser<TToken>
    {
        ParserHandler<TToken> parseHandler;
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
                return V.Seq<TToken>(value.ToCharArray().Select(c => V.Token((TToken)((object)c))).ToArray());
            else
                throw new NotImplementedException("This method only works when TToken is char.");
        }
    }
    public delegate ParseOutput<TToken> ParserHandler<TToken>(ParseInput<TToken> input);
    public class V
    {
        private static Parser<TToken> ProceedIf<TToken>(bool condition, Func<object> resultProvider)
        {
            return new Parser<TToken>(input =>
            {
                if (condition)
                    return new ParseOutput<TToken>(input.Input, input.Position + 1, true, resultProvider());
                else
                    return new ParseOutput<TToken>(input.Input, input.Position, false);
            });
        }
        private static Parser<TToken> ProceedIf<TToken>(Func<bool> condition, Func<object> resultProvider)
        {
            return new Parser<TToken>(input =>
                {
                    var success = condition();
                    if (success)
                        return new ParseOutput<TToken>(input.Input, input.Position + 1, true, resultProvider());
                    else
                        return new ParseOutput<TToken>(input.Input, input.Position, false);
                });
        }
        private static Parser<TToken> ProceedIf<TToken>(Func<TToken, bool> condition, Func<TToken, object> resultProvider)
        {
            return new Parser<TToken>(input =>
                {
                    var current = input.Current;
                    if (condition(current))
                        return new ParseOutput<TToken>(input.Input, input.Position + 1, true, resultProvider(current));
                    else
                        return new ParseOutput<TToken>(input.Input, input.Position, false);
                });
        }
        public static Parser<TToken> Token<TToken>(TToken expectedToken)
        {
            return ProceedIf<TToken>(currentToken => Comparer<TToken>.Default.Compare(currentToken, expectedToken) == 0, token => token);
        }
        public static Parser<TToken> EOI<TToken>()
        {
            return Token(default(TToken));
        }
        public static Parser<TToken> Seq<TToken>(params Parser<TToken>[] parsers)
        {
            return new Parser<TToken>(input =>
                {
                    var current = input;
                    var results = new List<object>();
                    foreach (var parser in parsers)
                    {
                        var output = parser.Run(current);
                        if (!output.Success)
                            return new ParseOutput<TToken>(input.Input, input.Position, false);
                        results.Add(output.Result);
                        current = new ParseInput<TToken>(output.Input, output.Position);
                    }
                    return new ParseOutput<TToken>(current.Input, current.Position, true, results.AsReadOnly());
                });
        }
        public static Parser<TToken> Any<TToken>(params Parser<TToken>[] parsers)
        {
            return new Parser<TToken>(input =>
                {
                    var current = input;
                    foreach (var parser in parsers)
                    {
                        var output = parser.Run(input);
                        if (output.Success)
                        {
                            return new ParseOutput<TToken>(output.Input, output.Position, true, output.Result);
                        }
                    }
                    return new ParseOutput<TToken>(input.Input, input.Position, false);
                });
        }
        public static Parser<TToken> ZeroOrMore<TToken>(Parser<TToken> parser)
        {
            return new Parser<TToken>(input =>
                {
                    var current = input;
                    var results = new List<object>();
                    while (true)
                    {
                        var output = parser.Run(current);
                        if (!output.Success)
                            break;
                        results.Add(output.Result);
                        current = new ParseInput<TToken>(output.Input, output.Position);
                        if (EOI<TToken>().Run(current).Success) // if reached to the end of input stream
                            break;
                    }
                    return new ParseOutput<TToken>(current.Input, current.Position, true, results.AsReadOnly());
                });
        }
        public static Parser<TToken> ZeroOrOne<TToken>(Parser<TToken> parser)
        {
            return new Parser<TToken>(input =>
                {
                    var output = parser.Run(input);
                    if (output.Success)
                        return new ParseOutput<TToken>(output.Input, output.Position, true, output.Result);
                    else
                        return new ParseOutput<TToken>(input.Input, input.Position, true, null);
                });
        }
        public static Parser<TToken> OneOrMore<TToken>(Parser<TToken> parser)
        {
            return new Parser<TToken>(input =>
                {
                    var current = input;
                    var results = new List<object>();
                    var output = parser.Run(input);
                    if (!output.Success)
                        return new ParseOutput<TToken>(input.Input, input.Position, false);
                    else
                        current = new ParseInput<TToken>(output.Input, output.Position);

                    results.Add(output.Result);

                    while (true)
                    {
                        output = parser.Run(current);
                        if (!output.Success)
                            break;
                        results.Add(output.Result);
                        current = new ParseInput<TToken>(output.Input, output.Position);
                        if (EOI<TToken>().Run(current).Success) // if reached to the end of input stream
                            break;
                    }
                    return new ParseOutput<TToken>(current.Input, current.Position, true, results.AsReadOnly());
                });
        }
    }
}
