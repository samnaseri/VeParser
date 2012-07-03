using System;
using System.Collections.Generic;

namespace VeParser
{
    /// <summary>
    /// The base class for a parser. This class provides basic combinators like any and seq that are required when creating more specific parsers.
    /// </summary>
    /// <typeparam name="TToken">The type of token, you can define your own type for the token or use string or char as token type.</typeparam>
    public partial class BaseParser<TToken>
    {
        protected ISourceNavigator<TToken> input;

        /// <summary>
        /// This method converts a TokenParser function into a Parser, so whenever you need a parser that acts on the current
        /// input token you may define a TokenParser and then convert it into a Parser.
        /// </summary>
        /// <param name="tokenParser">The token parser to be converted.</param>
        /// <param name="onSuccess">The action to be executed on the successful execution of token parser.</param>
        /// <returns>Returns a Parser which will call the provided TokenParser with the current input token.</returns>
        private Parser to_parser(TokenParser<TToken> tokenParser, Action<TToken> onSuccess)
        {
            Parser result = () =>
            {
                var next = input.getCurrent();
                if (InvokeTokenParser(tokenParser, next))
                {
                    if (onSuccess != null)
                        onSuccess(next);
                    input.MoveNext();
                    return true;
                }
                else
                {
                    return false;
                }
            };
            result.SetModel("Checking for token " + tokenParser.GetExpectedToken());
            result.SetActionDescription(tokenParser.GetExpectedToken());
            return result;
        }

        /// <summary>
        /// This method converts a TokenParser function into a Parser, so whenever you need a parser that acts on the current
        /// input token you may define a TokenParser and then convert it into a Parser.
        /// </summary>
        /// <param name="tokenParser">The token parser to be converted.</param>
        /// <param name="onSuccess">The action to be executed on the successful execution of token parser.</param>
        protected Parser toParser(TokenParser<TToken> tokenParser)
        {
            return to_parser(tokenParser, null);
        }

        #region Basic Combinators

        /// <summary>
        /// Returns a token parser function which requires the token to be equal to the specified token value.
        /// </summary>
        /// <param name="token">The required token.</param>
        /// <returns>A token parser function.</returns>
        protected TokenParser<TToken> tokenParser(TToken token)
        {
            TokenParser<TToken> result = (next) => Comparer<TToken>.Default.Compare(next, token) == 0;
            result.SetExpectedToken(token.ToString());
            return result;
        }

        protected Parser token(TToken t)
        {
            var result = toParser(tokenParser(t));
            result.SetModel("Expecting token of " + t.ToString());
            result.SetActionDescription(t.ToString());
            return result;
        }

        protected Parser endOfFile()
        {
            Parser result = () => input.IsEndOfFile;
            result.SetModel("Expect to be at end of file");
            return result;
        }

        #endregion Basic Combinators
    }
}