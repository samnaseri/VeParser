using System.Collections.Generic;

namespace VeParser
{
    public partial class BaseParser<TToken>
    {
        //
        // These methods internally will be called instead of directly invoking a parser or a tokenchecker or they will be called
        // whenevery a parsing branch begins or ends. So the derived parser types can hook these operations for monitoring, debugging
        // or even changing the way parsers behave.
        //

        protected virtual void beginSubParsers(IEnumerable<Parser> subParsers)
        {
        }

        protected virtual void endSubParsers()
        {
        }

        protected virtual bool invokeParser(Parser parser)
        {
            return parser();
        }

        protected virtual bool invokeTokenChecker(TokenChecker<TToken> tokenChecker , TToken token)
        {
            return tokenChecker(token);
        }
    }
}