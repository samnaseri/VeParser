
using System;
using System.Collections.Generic;
namespace VeParser.Samples
{
    // Before lexification we read char by char, but now language parts are tokens(string)
    // A note about method names:
    //  methods which name is started with 'expect' only check if the current token is of expect type and value or not.
    //  method which name is not started with 'expect' also check if the current token is of expected type, but they can be used in calls to fill method

    public abstract partial class TokenParser : BaseParser<Token>
    {
        public TokenParser()
        {
            identifier.SetExpectedToken("{an identifier}");
            number.SetExpectedToken("{a number}");
            keyword.SetExpectedToken("{any keyword}");
            text.SetExpectedToken("{a text}");

            expectIdentifier = toParser(identifier);
            expectNumber = toParser(number);
            expectText = toParser(text);
            expectKeyword = toParser(keyword);
        }

        protected TokenParser<Token> identifier = next => next is Identifier;
        protected Parser expectIdentifier;
        protected TokenParser<Token> number = next => next is Number;
        protected Parser expectNumber;
        protected TokenParser<Token> text = next => next is Text;
        protected Parser expectText;
        protected TokenParser<Token> keyword = next => next is Keyword;
        protected Parser expectKeyword;

        protected TokenParser<Token> symbol_of(string symbol)
        {
            TokenParser<Token> result = next => next is Symbol && next.Name == symbol;
            result.SetExpectedToken("{symbol " + symbol + "}");
            return result;
        }

        protected Parser expectSymbol_of(string symbol)
        {
            return toParser(symbol_of(symbol));
        }

        protected TokenParser<Token> keyword_of(string keyword)
        {
            TokenParser<Token> result = next => next is Keyword && next.Name == keyword;
            result.SetExpectedToken("{keyword " + keyword + "}");
            return result;
        }

        protected Parser expectKeyword_of(string keyword)
        {
            return toParser(keyword_of(keyword));
        }

        protected Parser parenthesis_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("("), rule, expectSymbol_of(")"));
        }

        protected Parser squreBracket_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("["), rule, expectSymbol_of("]"));
        }

        protected Parser braces_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("{"), rule, expectSymbol_of("}"));
        }

        protected Parser angleBracket_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("<"), rule, expectSymbol_of(">"));
        }
    }

    public abstract partial class DebugingTokenParser : TokenParser
    {
        public new Parser expectKeyword_of(string keyword)
        {
            return alert("looking for " + keyword + " keyword",
                () =>
                {
                    alertCurrent();
                    var result = base.expectKeyword_of(keyword)();
                    Console.WriteLine(result ? "Keyword Found" : "Keyword Not Found");
                    return result;
                });
        }


        /// <summary>
        /// To temporarily ignore a parser, so it would return a fixed result.        
        /// </summary>
        /// <param name="action"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Parser ignore(Parser action, bool result) { return () => { action(); return result; }; }

        #region Alert functions
        /// <summary>
        /// To see the current token in the console for debugging purposes.
        /// </summary>
        public void alertCurrent() { Console.WriteLine(input.getCurrent()); }

        public Parser alert(string message) { return () => { Console.WriteLine(message); return false; }; }

        public Parser alert(string message, Parser parser) { return any(ignore(alert(message), true), parser); }
        #endregion

        #region Monitoring
        int currentIndent = 0;

        protected override void BeginSubParsers(IEnumerable<Parser> subParsers)
        {
            base.BeginSubParsers(subParsers);
            currentIndent = currentIndent + 1;
        }

        protected override void EndSubParsers()
        {
            EndSubParsers();
            currentIndent = currentIndent - 1;
        }

        protected override bool InvokeParser(Parser parser)
        {
            var description = parser.GetModel().GetFullDefinition();
            Console.WriteLine((currentIndent.ToString().PadLeft(currentIndent, ' ') + "Running parser : " + description).PadRight(currentIndent, ' '));
            Console.ReadLine();
            var result = parser();
            Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent, ' ') + (result ? "Passed" : "Failed"));
            return result;
        }

        protected override bool InvokeTokenParser(TokenParser<Token> tokenParser, Token token)
        {
            var expectation = tokenParser.GetExpectedToken();
            if (expectation != null)
                Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent, ' ') + "looking for " + expectation);
            return tokenParser(token);
        }
        #endregion
    }
}