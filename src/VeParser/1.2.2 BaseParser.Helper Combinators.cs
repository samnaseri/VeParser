
using System;
using System.Collections.Generic;
namespace VeParser
{
    public abstract partial class DebugingParser<TToken> : BaseParser<TToken>
    {
        // PURPOSE :
        // These methods provide some monitoring features you may use for debugging your parser.
        //       


        /// <summary>
        /// To temporarily ignore a parser, so it would return a fixed result.        
        /// </summary>
        /// <param name="action">The parser to be ignored. you may pass a function that do some not parsing works, for example writing something to Console for debuging.</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Parser ignore(Parser parser, bool result)
        {
            return () => { parser(); return result; };
        }

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

        protected override bool InvokeTokenParser(TokenParser<TToken> tokenParser, TToken token)
        {
            var expectation = tokenParser.GetExpectedToken();
            if (expectation != null)
                Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent, ' ') + "looking for " + expectation);
            return tokenParser(token);
        }
        #endregion
    }
}
