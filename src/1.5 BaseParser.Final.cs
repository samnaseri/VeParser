using System;
using System.Collections.Generic;

namespace VeParser
{
    public abstract partial class BaseParser<TToken>
    {
        protected abstract Parser GetRootParser();
        
        public object Parse(IEnumerable<TToken> input)
        {
            var root_rule = GetRootParser();
            if (root_rule == null)
                throw new InvalidParserException("The root parser could not be null.",null);

            try {
                parseTree = new ParseTree();
                this.source = new ParallelSourceNavigator<TToken>(new SourceReader<TToken>(input.GetEnumerator()));

                var parsed = invokeParser(root_rule);
                if (parsed) {
                    var result = parseTree.Pop();
                    return result;
                }
                return null;
            } catch (InvalidOperationException ex) {
                if (ex.Message == "Stack empty.") {
                    throw new InvalidParserException("A fill method tried to get a value that actually is not created. You can call fill method when the called parser contains a create function call. Thats it, the create function should be called then the fill method will use the created value to fill the property." , ex);
                }
                throw;
            }
        }        
    }
}