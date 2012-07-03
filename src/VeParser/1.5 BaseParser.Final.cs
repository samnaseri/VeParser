using System;
using System.Collections;
using System.Collections.Generic;

namespace VeParser
{
    public abstract partial class BaseParser<TToken>
    {
        protected abstract Parser GetRootParser();

        protected ISourceNavigator<TToken> CreateSourceNavigator(IEnumerable<TToken> source)
        {
            return new SourceNavigator<TToken>(new EnumeratorReader<TToken>(source));
        }

        public object Parse(IEnumerable<TToken> input)
        {
            var root_rule = GetRootParser();
            if (root_rule == null)
                throw new InvalidParserException("The root parser could not be null.", null);

            try
            {
                output = new Stack();
                this.input = CreateSourceNavigator(input);

                var parsed = InvokeParser(root_rule);
                if (parsed)
                {
                    var result = output.Pop();
                    return result;
                }
                return null;
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "Stack empty.")
                {
                    throw new InvalidParserException("A consume method(replaceOutputXXX,add*,set*,...) tried to get a value that actually is not created. You can call fill method when the called parser contains a create function call. Thats it, the create function should be called then the fill method will use the created value to fill the property.", ex);
                }
                throw;
            }
        }
    }
}