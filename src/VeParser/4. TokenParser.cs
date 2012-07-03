using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    // Before lexification we read char by char, but now language parts are tokens(string)
    // A note about method names:
    //  methods which name is started with 'expect' only check if the current token is of expect type and value or not.
    //  method which name is not started with 'expect' also check if the current token is of expected type, but they can be used in calls to fill method

    public abstract partial class TokenParser : BaseParser<token>
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

        protected TokenChecker<token> identifier = next => next is identifier;
        protected Parser expectIdentifier;
        protected TokenChecker<token> number = next => next is number;
        protected Parser expectNumber;
        protected TokenChecker<token> text = next => next is text;
        protected Parser expectText;
        protected TokenChecker<token> keyword = next => next is keyword;
        protected Parser expectKeyword;

        protected TokenChecker<token> symbol_of(string symbol)
        {
            TokenChecker<token> result = next => next is symbol && next.GetString() == symbol;
            result.SetExpectedToken("{symbol " + symbol + "}");
            return result;
        }

        protected Parser expectSymbol_of(string symbol)
        {
            return toParser(symbol_of(symbol));
        }

        protected TokenChecker<token> keyword_of(string keyword)
        {
            TokenChecker<token> result = next => next is keyword && next.GetString() == keyword;
            result.SetExpectedToken("{keyword " + keyword + "}");
            return result;
        }

        protected Parser expectKeyword_of(string keyword)
        {
            return toParser(keyword_of(keyword));
        }

        protected Parser parenthesis_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("(") , rule , expectSymbol_of(")"));
        }

        protected Parser squreBracket_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("[") , rule , expectSymbol_of("]"));
        }

        protected Parser braces_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("{") , rule , expectSymbol_of("}"));
        }

        protected Parser angleBracket_enclosed(Parser rule)
        {
            return seq(expectSymbol_of("<") , rule , expectSymbol_of(">"));
        }
    }


}