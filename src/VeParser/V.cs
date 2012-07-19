namespace VeParser
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Define a set of parsing operators.
    /// </summary>
    public static class V
    {
        internal static Parser<TToken> ProceedIf<TToken>(Func<TToken, bool> condition)
        {
            return new Parser<TToken>((context, position) =>
            {
                var current = context.Current(position);
                if (condition(current))
                    return new ParseOutput<TToken>((ushort)(position + 1), current);
                else
                    return null;
            });
        }
        public static Parser<TToken> Token<TToken>(TToken expectedToken)
        {
            return new TokenParser<TToken>(expectedToken);
        }
        public static Parser<TToken> EOI<TToken>()
        {
            return Token(default(TToken));
        }
        public static Parser<TToken> Seq<TToken>(params Parser<TToken>[] parsers)
        {
            return new SeqParser<TToken>(parsers);
        }
        public static Parser<TToken> Any<TToken>(params Parser<TToken>[] parsers)
        {
            return new AnyParser<TToken>(parsers);
        }
        public static Parser<TToken> PAny<TToken>(params Parser<TToken>[] parsers)
        {
            var handlers = parsers.Select(p => p.parseHandler).ToArray();
            return new Parser<TToken>((context, position) =>
            {
                return handlers.AsParallel().Select(handler => handler(context, position)).FirstOrDefault(i => i != null);
            });
        }
        public static Parser<TToken> ZeroOrMore<TToken>(Parser<TToken> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            return new Parser<TToken>((context, position) =>
            {
                var currentPosition = position;
                var results = new List<object>();
                while (true)
                {
                    var output = handler(context, currentPosition);
                    if (output == null)
                        break;
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                        break;
                }
                return new ParseOutput<TToken>(currentPosition, results);
            });
        }
        public static Parser<TToken> ZeroOrOne<TToken>(Parser<TToken> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            return new Parser<TToken>((context, position) =>
            {
                var output = handler(context, position);
                if (output != null)
                    return output;
                else
                    return new ParseOutput<TToken>(position, null);
            });
        }
        public static Parser<TToken> OneOrMore<TToken>(Parser<TToken> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            return new Parser<TToken>((context, position) =>
            {
                var currentPosition = position;
                var results = new List<object>();
                var output = handler(context, position);
                if (output == null)
                    return null;
                else
                    currentPosition = output.Position;
                results.Add(output.Result);
                while (true)
                {
                    output = handler(context, currentPosition);
                    if (output == null)
                        break;
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    if (context.Current(currentPosition) == null)// if reached to the end of input stream
                        break;
                }
                return new ParseOutput<TToken>(currentPosition, results);
            });
        }
        public static Parser<TToken> Repeat<TToken>(Parser<TToken> parser, int? minRepeatCount, int? maxRepeatCount)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            if (minRepeatCount != null && maxRepeatCount != null)
            {
                return new Parser<TToken>((context, position) =>
                {
                    var currentPosition = position;
                    var results = new List<object>();
                    int count = 0;
                    for (; count < minRepeatCount; count++)
                    {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return null;
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    for (; count < maxRepeatCount; count++)
                    {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return new ParseOutput<TToken>(currentPosition, results);
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    return null;
                });
            }
            else if (minRepeatCount != null)
            {
                return new Parser<TToken>((context, position) =>
                {
                    var currentPosition = position;
                    var results = new List<object>();
                    int count = 0;
                    for (; count < minRepeatCount; count++)
                    {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return null;
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    for (; true; )
                    {
                        var output = handler(context, currentPosition);
                        if (output == null || Object.Equals(context.Current(currentPosition), default(TToken)))
                            return new ParseOutput<TToken>(currentPosition, results);
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                });
            }
            else
            {
                return new Parser<TToken>((context, position) =>
                {
                    var currentPosition = position;
                    var results = new List<object>();
                    int count = 0;
                    for (; count < maxRepeatCount; count++)
                    {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return new ParseOutput<TToken>(currentPosition, results);
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    return null;
                });
            }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006")]
        public static Parser<TToken> Scope<TToken, TParsers, TOutput>(Func<Dictionary<string, object>, TParsers> resultParsers, Func<TParsers, Parser<TToken>> combinator, Func<Dictionary<string, object>, TOutput> outputProjection)
        {
            return new Parser<TToken>((context, position) =>
            {
                var dic = new Dictionary<string, object>();
                var output = combinator(resultParsers(dic)).Run(context, position);

                if (output != null)
                    return new ParseOutput<TToken>(output.Position, outputProjection(dic));

                return output;
            });
        }
        public static Parser<TToken> ScopeParser<TToken>(Dictionary<string, object> dic, string value, Parser<TToken> parser)
        {
            return new Parser<TToken>((context, position) =>
            {
                var output = parser.Run(context, position);
                if (output != null)
                    dic[value] = output.Result;
                return output;
            });
        }
        public static Parser<TToken> Not<TToken>(Func<TToken, bool> condition)
        {
            return V.ProceedIf<TToken>(c => !condition(c));
        }
        public static Parser<TToken> DelimitedList<TToken>(Parser<TToken> listItem, Parser<TToken> delimiter, bool acceptEmptyList)
        {
            return new Parser<TToken>((context, position) =>
            {
                var currentPosition = position;
                var results = new List<object>();
                while (true)
                {
                    var output = listItem.Run(context, currentPosition);
                    if (output == null)
                    {
                        if (results.Count == 0 && acceptEmptyList)
                            return new ParseOutput<TToken>(position, null);
                        else
                            return null;
                    }
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    var delimiterOutput = delimiter.Run(context, currentPosition);
                    if (delimiterOutput == null)
                        break;
                    currentPosition = delimiterOutput.Position;
                }
                return new ParseOutput<TToken>(currentPosition, results);
            });
        }
    }
}
