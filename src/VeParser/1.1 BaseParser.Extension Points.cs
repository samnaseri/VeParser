using System.Collections.Generic;

namespace VeParser
{
    public partial class BaseParser<TToken>
    {
        // PURPOSE :
        // These methods internally will be called instead of directly invoking a parser or a token parser or they will be called
        // whenevery a parsing branch begins or ends. So the derived parser types can hook these operations for monitoring, debugging
        // or even changing the way parsers behave.
        //

        /// <summary>
        /// Will be called when a combinator like seq or any which processes a bunch of parsers starts.
        /// </summary>
        /// <param name="subParsers">The parsers that are going to be processed.</param>
        protected virtual void BeginSubParsers(IEnumerable<Parser> subParsers)
        {
        }

        /// <summary>
        /// Will be called when a combinator like seq or any which processes a bunch of parsers finishes.
        /// </summary>
        protected virtual void EndSubParsers()
        {
        }

        /// <summary>
        /// Will be called when a parser is going to be executed. You can override this method to monitor or control the
        /// execution of every parser, or you may override it for debugging purposes.
        /// </summary>
        /// <remarks>
        /// Notes for overriding: You may call the base method and return its value, so you have not changed the parser execution process. But for some special
        /// scenarios you may change the behaviour of parser engine by not calling the parser, for example if you want to see if a special parser is causing parsing problems
        /// you may ignore its execution when overriding this method.
        /// </remarks>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected virtual bool InvokeParser(Parser parser)
        {
            return parser();
        }

        /// <summary>
        /// Will be called when a toekn parser is going to be executed. You can override this method to monitor or control the
        /// execution of every token parser, or you may override it for debugging purposes.
        /// </summary>
        /// <param name="tokenParser">The token parser function.</param>
        /// <param name="token">The token to be passed to the token parser.</param>
        /// <returns></returns>
        protected virtual bool InvokeTokenParser(TokenParser<TToken> tokenParser, TToken token)
        {
            return tokenParser(token);
        }
    }
}