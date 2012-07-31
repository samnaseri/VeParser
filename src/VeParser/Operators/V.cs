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
        public static Parser<TToken> If<TToken>(Func<TToken, bool> condition)
        {
            return new Parser<TToken>((context, position) => {
                var current = context.Current(position);
                if (condition(current))
                    return new ParseOutput<TToken>(position + 1, current);
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
            return new Parser<TToken>((context, position) => {
                return handlers.AsParallel().Select(handler => handler(context, position)).FirstOrDefault(i => i != null);
            });
        }
        public static Parser<TToken> ZeroOrMore<TToken>(Parser<TToken> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            return new Parser<TToken>((context, position) => {
                var currentPosition = position;
                var results = new List<object>();
                while (true) {
                    var output = handler(context, currentPosition);
                    if (output == null)
                        break;
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                        break;
                }
                return new ParseOutput<TToken>(currentPosition, results.ToArray());
            });
        }
        public static Parser<TToken> ZeroOrOne<TToken>(Parser<TToken> parser)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            return new Parser<TToken>((context, position) => {
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
            return new Parser<TToken>((context, position) => {
                var currentPosition = position;
                var results = new List<object>();
                var output = handler(context, position);
                if (output == null)
                    return null;
                else
                    currentPosition = output.Position;
                results.Add(output.Result);
                while (true) {
                    output = handler(context, currentPosition);
                    if (output == null)
                        break;
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    if (context.Current(currentPosition) == null)// if reached to the end of input stream
                        break;
                }
                return new ParseOutput<TToken>(currentPosition, results.ToArray());
            });
        }
        public static Parser<TToken> Repeat<TToken>(Parser<TToken> parser, int? minRepeatCount, int? maxRepeatCount)
        {
            if (parser == null)
                throw new ArgumentNullException("parser");
            var handler = parser.parseHandler;
            if (minRepeatCount != null && maxRepeatCount != null) {
                return new Parser<TToken>((context, position) => {
                    var currentPosition = position;
                    var results = new List<object>();
                    int count = 0;
                    for (; count < minRepeatCount; count++) {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return null;
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    for (; count < maxRepeatCount; count++) {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return new ParseOutput<TToken>(currentPosition, results.ToArray());
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    return null;
                });
            }
            else if (minRepeatCount != null) {
                return new Parser<TToken>((context, position) => {
                    var currentPosition = position;
                    var results = new List<object>();
                    int count = 0;
                    for (; count < minRepeatCount; count++) {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return null;
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                    for (; true; ) {
                        var output = handler(context, currentPosition);
                        if (output == null || Object.Equals(context.Current(currentPosition), default(TToken)))
                            return new ParseOutput<TToken>(currentPosition, results.ToArray());
                        results.Add(output.Result);
                        currentPosition = output.Position;
                    }
                });
            }
            else {
                return new Parser<TToken>((context, position) => {
                    var currentPosition = position;
                    var results = new List<object>();
                    int count = 0;
                    for (; count < maxRepeatCount; count++) {
                        var output = handler(context, currentPosition);
                        if (output == null)
                            return new ParseOutput<TToken>(currentPosition, results.ToArray());
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
            return new Parser<TToken>((context, position) => {
                var dic = new Dictionary<string, object>();
                var output = combinator(resultParsers(dic)).Run(context, position);

                if (output != null)
                    return new ParseOutput<TToken>(output.Position, outputProjection(dic));

                return output;
            });
        }
        public static Parser<TToken> ScopeParser<TToken>(Dictionary<string, object> dic, string value, Parser<TToken> parser)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null)
                    dic[value] = output.Result;
                return output;
            });
        }
        public static Parser<TToken> Not<TToken>(Func<TToken, bool> condition)
        {
            return V.If<TToken>(c => !condition(c));
        }
        public static Parser<TToken> DelimitedList<TToken>(Parser<TToken> listItem, Parser<TToken> delimiter, bool acceptEmptyList)
        {
            return new Parser<TToken>((context, position) => {
                var currentPosition = position;
                var results = new List<object>();
                while (true) {
                    var output = listItem.Run(context, currentPosition);
                    if (output == null) {
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
                return new ParseOutput<TToken>(currentPosition, results.ToArray());
            });
        }

        public static LateDefinedParser<TToken> TemporaryReference<TToken>()
        {
            return new LateDefinedParser<TToken>();
        }

        public static Parser<TToken> Capture<TToken>(Func<object, object> captureFunc, Parser<TToken> parser)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                output = new ParseOutput<TToken>(output.Position, captureFunc(output.Result));
                return output;
            });
        }

        public static Parser<TToken> TakeWhile<TToken>(Parser<TToken> condition, Parser<TToken> parser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (parser == null)
                throw new ArgumentNullException("parser");

            return new Parser<TToken>((context, position) => {
                var initialPosition = position;
                int lastPosition = position;
                {
                    var currentPosition = position;
                    var results = new List<object>();
                    while (true) {
                        var output = condition.Run(context, currentPosition);
                        if (output == null)
                            break;
                        results.Add(output.Result);
                        currentPosition = output.Position;
                        if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                            break;
                    }
                    lastPosition = currentPosition;
                }
                if (lastPosition > initialPosition) {
                    // perform parser over sequence of tokens from initialPosition until lastPosition
                    var subContext = new WindowedParseContext<TToken>(context, initialPosition, lastPosition);
                    return parser.Run(subContext, initialPosition);
                }
                else
                    return new ParseOutput<TToken>(lastPosition, null);
            });
        }
        public static Parser<TToken> SkipWhile<TToken>(Parser<TToken> condition)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            return new Parser<TToken>((context, position) => {
                var currentPosition = position;
                var results = new List<object>();
                while (true) {
                    var output = condition.Run(context, currentPosition);
                    if (output == null)
                        break;
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                        break;
                }
                return new ParseOutput<TToken>(currentPosition, null);
            });
        }
        public static Parser<TToken> TakeWhile<TToken>(Func<TToken, bool> condition, Parser<TToken> parser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (parser == null)
                throw new ArgumentNullException("parser");

            return new Parser<TToken>((context, position) => {
                var initialPosition = position;
                int lastPosition = position;
                {
                    var currentPosition = position;
                    var results = new List<object>();
                    while (true) {
                        var matches = condition(context.Current(currentPosition));
                        if (matches == null)
                            break;
                        currentPosition++;
                        if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                            break;
                    }
                    lastPosition = currentPosition;
                }
                if (lastPosition > initialPosition) {
                    // perform parser over sequence of tokens from initialPosition until lastPosition
                    var subContext = new WindowedParseContext<TToken>(context, initialPosition, lastPosition);
                    return parser.Run(subContext, initialPosition);
                }
                else
                    return new ParseOutput<TToken>(lastPosition, null);
            });
        }
        public static Parser<TToken> TakeUntil<TToken>(Parser<TToken> condition, Parser<TToken> parser)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            if (parser == null)
                throw new ArgumentNullException("parser");
            return new Parser<TToken>((context, position) => {
                var initialPosition = position;
                int lastPosition = position;
                {
                    var currentPosition = position;
                    var results = new List<object>();
                    while (true) {
                        var output = condition.Run(context, currentPosition);
                        if (output != null)
                            break;
                        results.Add(output.Result);
                        currentPosition = output.Position;
                        if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                            break;
                    }
                    lastPosition = currentPosition;
                }
                if (lastPosition > initialPosition) {
                    // perform parser over sequence of tokens from initialPosition until lastPosition
                    var subContext = new WindowedParseContext<TToken>(context, initialPosition, lastPosition);
                    return parser.Run(subContext, initialPosition);
                }
                else
                    return new ParseOutput<TToken>(lastPosition, null);
            });
        }
        public static Parser<TToken> SkipUntil<TToken>(Parser<TToken> condition)
        {
            if (condition == null)
                throw new ArgumentNullException("condition");
            return new Parser<TToken>((context, position) => {
                var currentPosition = position;
                var results = new List<object>();
                while (true) {
                    var output = condition.Run(context, currentPosition);
                    if (output != null)
                        break;
                    results.Add(output.Result);
                    currentPosition = output.Position;
                    if (Object.Equals(context.Current(currentPosition), default(TToken)))// if reached to the end of input stream                        
                        break;
                }
                return new ParseOutput<TToken>(currentPosition, null);
            });
        }


    }
    public class LateDefinedParser<TToken> : Parser<TToken>
    {
        Parser<TToken> definition;
        public LateDefinedParser()
            : base(null)
        {
            parseHandler = (context, position) => { return definition.parseHandler(context, position); };
        }
        public void SetParser(Parser<TToken> definition)
        {
            this.definition = definition;
        }
    }

}
