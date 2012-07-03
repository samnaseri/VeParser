using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    public partial class BaseParser<TToken>
    {
        // PORPUSE : 
        // Circular Reference and Recursive Parser support

        Dictionary<Parser, Guid> recParserRefs = new Dictionary<Parser, Guid>();
        Dictionary<Guid, Parser> recParserDefs = new Dictionary<Guid, Parser>();
        Dictionary<Guid, int> recursionDepths = new Dictionary<Guid, int>();
        int maximum_recursion_depth = 100;

        /// <summary>
        /// Creates a parser which its body definition can be defined later using <see cref="setReference"/> method.
        /// </summary>
        /// <returns>The parser reference.</returns>
        protected Parser createReference()
        {
            var guid = Guid.NewGuid();
            Parser result = () =>
            {
                if (recursionDepths[guid] > maximum_recursion_depth)
                    throw new InvalidParserException("Reached the maximum allowed recursion depth, The parser definition is invalid, it may contains some sort of direct or indirect left recursion.", null);
                recursionDepths[guid] = recursionDepths[guid] + 1;
                return recParserDefs[guid]();
            };
            recursionDepths.Add(guid, 0);
            recParserRefs.Add(result, guid);
            return result;
        }

        /// <summary>
        /// Sets the parser body. By calling <see cref="createReference" /> you will get a parser reference. you can use this parser reference by creating some
        /// parsers that uses it. Acheving a more complex parser using your parser reference, you can set the more complex parser as the body of your parser
        /// reference. By doing so you have a parser that is combined by itself. This will be useful for writing parsers for expressions in most of programming languages.       
        /// </summary>
        /// <param name="reference">The reference to be set.</param>
        /// <param name="body">The body parser, usally it is acheived by combining some parsers including the reference. Otherwise probably you should not used this setReference method.</param>
        protected void setReference(Parser reference, Parser body)
        {
            var guid = recParserRefs[reference];
            recParserDefs[guid] = body;
            var parserIndex = recParserRefs.Keys.ToList().IndexOf(reference);
            reference.SetModel("Recursion of #" + parserIndex);
            var bodyDefinition = body.GetModel().GetFullDefinition();
            reference.SetModel("Parser #" + parserIndex + ":" + bodyDefinition);
        }
    }
}