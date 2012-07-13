// -----------------------------------------------------------------------
// <copyright file="0.Basics.cs" company="">
// 
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections;
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
        private static ParseOutput<TToken> ToSuccess<TToken>(ParseInput<TToken> input, bool nextPosition, object result)
        {
            return new ParseOutput<TToken>(input.Input, input.Position + (nextPosition ? 1 : 0), true, result);
        }
        private static ParseOutput<TToken> ToFail<TToken>(ParseInput<TToken> input)
        {
            return new ParseOutput<TToken>(input.Input, input.Position, false);
        }
        private static ParseOutput<TToken> Clone<TToken>(ParseOutput<TToken> output)
        {
            return new ParseOutput<TToken>(output.Input, output.Position, output.Success, output.Result);
        }
        private static ParseInput<TToken> Continue<TToken>(ParseOutput<TToken> output)
        {
            return new ParseInput<TToken>(output.Input, output.Position);
        }
        private static Parser<TToken> ProceedIf<TToken>(bool condition, Func<object> resultProvider)
        {
            return new Parser<TToken>(input =>
            {
                if (condition)
                    return ToSuccess(input, true, resultProvider());
                else
                    return ToFail(input);
            });
        }
        private static Parser<TToken> ProceedIf<TToken>(Func<bool> condition, Func<object> resultProvider)
        {
            return new Parser<TToken>(input =>
                {
                    var success = condition();
                    if (success)
                        return ToSuccess(input, true, resultProvider());
                    else
                        return ToFail(input);
                });
        }
        private static Parser<TToken> ProceedIf<TToken>(Func<TToken, bool> condition, Func<TToken, object> resultProvider)
        {
            return new Parser<TToken>(input =>
                {
                    var current = input.Current;
                    if (condition(current))
                        return ToSuccess(input, true, resultProvider(current));
                    else
                        return ToFail(input);
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
                            return ToFail(input);
                        results.Add(output.Result);
                        current = Continue(output);
                    }
                    return ToSuccess(current, false, results.AsReadOnly());
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
                            return Clone(output);
                        }
                    }
                    return ToFail(input);
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
                        current = Continue(output);
                        if (EOI<TToken>().Run(current).Success) // if reached to the end of input stream
                            break;
                    }
                    return ToSuccess(current, false, results.AsReadOnly());
                });
        }
        public static Parser<TToken> ZeroOrOne<TToken>(Parser<TToken> parser)
        {
            return new Parser<TToken>(input =>
                {
                    var output = parser.Run(input);
                    if (output.Success)
                        return Clone(output);
                    else
                        return ToSuccess(input, false, null);
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
                        return ToFail(input);
                    else
                        current = Continue(output);

                    results.Add(output.Result);

                    while (true)
                    {
                        output = parser.Run(current);
                        if (!output.Success)
                            break;
                        results.Add(output.Result);
                        current = Continue(output);
                        if (EOI<TToken>().Run(current).Success) // if reached to the end of input stream
                            break;
                    }
                    return ToSuccess(current, false, results.AsReadOnly());
                });
        }
        public static Parser<TToken> Scope<TToken, TParsers>(TParsers resultParsers, Func<TParsers, Parser<TToken>> combinator)
        {
            return new Parser<TToken>(input =>
            {
                var properties = resultParsers.AsDictionary<object>();
                var resultsDictionary = (from p in properties
                                         where !(p.Value is Parser<TToken>)
                                         select new { Name = p.Key, Value = p.Value })
                                        .ToDictionary(i => i.Name, i => i.Value);
                var injectedParsersDictionary = (from p in properties
                                                 where p.Value is Parser<TToken>
                                                 select new
                                                 {
                                                     Name = p.Key,
                                                     Value = new Parser<TToken>(pInput =>
                                                     {
                                                         var pOutout = ((Parser<TToken>)p.Value).Run(pInput);
                                                         if (resultsDictionary.ContainsKey(p.Key))
                                                         {
                                                             var list = resultsDictionary[p.Key] as IList;
                                                             if (list == null)
                                                                 list = new ArrayList();
                                                             list.Add(pOutout.Result);
                                                         }
                                                         else
                                                             resultsDictionary[p.Key] = pOutout.Result;
                                                         return pOutout;
                                                     })
                                                 }).ToDictionary(i => i.Name, i => i.Value);
                resultParsers.FromDictionary(injectedParsersDictionary);

                var output = combinator(resultParsers).Run(input);
                if (output.Success)
                {
                    return new ParseOutput<TToken>(output.Input, output.Position, true, resultsDictionary);
                }
                return output;
            });
        }
        public static Parser<TToken> Not<TToken>(Parser<TToken> parser)
        {
            return new Parser<TToken>(input =>
                {
                    var output = parser.Run(input);
                    if (output.Success)
                        return ToFail(input);
                    else
                        return ToSuccess(input, false, null);
                });
        }
        public static Parser<TToken> DelimitedList<TToken>(Parser<TToken> listItem, Parser<TToken> delimiter, bool acceptEmptyList)
        {
            return new Parser<TToken>(input =>
            {
                var current = input;
                var results = new List<object>();

                while (true)
                {
                    var output = listItem.Run(current);
                    if (!output.Success)
                        return (results.Count == 0 && acceptEmptyList) ? ToSuccess(input, false, null) : ToFail(input);
                    results.Add(output.Result);
                    current = new ParseInput<TToken>(output.Input, output.Position);

                    var delimiterOutput = delimiter.Run(current);
                    if (!delimiterOutput.Success)
                        break;
                    current = Continue(delimiterOutput);
                }
                return ToSuccess(current, false, results.AsReadOnly());
            });
        }
    }
}
