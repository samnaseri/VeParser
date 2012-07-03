using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    public partial class BaseParser<TToken>
    {
        #region Circular Reference and Recursive Parser support

        Dictionary<Parser,Guid> recParserRefs = new Dictionary<Parser , Guid>();
        Dictionary<Guid,Parser> recParserDefs = new Dictionary<Guid , Parser>();
        Dictionary<Guid, int> recursionDepths = new Dictionary<Guid , int>();
        int maximum_recursion_depth = 100;

        protected Parser createReference()
        {
            var guid = Guid.NewGuid();
            Parser result = () => {
                if (recursionDepths[guid] > maximum_recursion_depth)
                    throw new InvalidParserException("Reached the maximum allowed recursion depth, The parser definition is invalid, it may contains some sort of direct or indirect left recursion." , null);
                recursionDepths[guid] = recursionDepths[guid] + 1;
                return recParserDefs[guid]();
            };
            recursionDepths.Add(guid , 0);
            recParserRefs.Add(result , guid);
            return result;
        }

        protected void setReference(Parser parser , Parser body)
        {
            var guid = recParserRefs[parser];
            recParserDefs[guid] = body;
            var parserIndex = recParserRefs.Keys.ToList().IndexOf(parser);
            parser.SetModel("Recursion of #" + parserIndex);
            var bodyDefinition = body.GetModel().GetFullDefinition();
            parser.SetModel("Parser #" + parserIndex + ":" + bodyDefinition);
        }

        #endregion Circular Reference and Recursive Parser support
    }
}