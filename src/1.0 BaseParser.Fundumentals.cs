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
        protected ParallelSourceNavigator<TToken> source;

        private Parser to_parser(TokenChecker<TToken> tokenChecker , Action<TToken> onSuccess)
        {
            Parser result = () => {
                var next = source.getCurrent();
                if (invokeTokenChecker(tokenChecker , next)) {
                    if (onSuccess != null)
                        onSuccess(next);
                    source.MoveNext();
                    return true;
                }
                else {
                    return false;
                }
            };
            result.SetModel("Checking for token " + tokenChecker.GetExpectedToken());
            result.SetActionDescription(tokenChecker.GetExpectedToken());
            return result;
        }

        protected Parser toParser(TokenChecker<TToken> tokenChecker)
        {
            return to_parser(tokenChecker , null);
        }

        #region Basic Combinators

        /// <summary>
        /// Returns a token checker function which requires the token to be equal to the specified token value.
        /// </summary>
        /// <param name="token">The required token.</param>
        /// <returns>A token checker function.</returns>
        protected TokenChecker<TToken> token(TToken token)
        {
            TokenChecker<TToken> result = (next) => Comparer<TToken>.Default.Compare(next , token) == 0;
            result.SetExpectedToken(token.ToString());
            return result;
        }

        protected Parser Token(TToken t)
        {
            var result = toParser(token(t));
            result.SetModel("Expecting token of " + t.ToString());
            result.SetActionDescription(t.ToString());
            return result;
        }

        protected Parser empty()
        {
            Parser result = () => true;
            result.SetModel("Empty for skip");
            return result;
        }

        protected Parser end_of_file()
        {
            Parser result = () => source.IsEndOfFile;
            result.SetModel("Expect to be at end of file");
            return result;
        }

        #endregion Basic Combinators
    }
}