
using System;
using System.Collections.Generic;
namespace VeParser
{
    public abstract partial class DebugingParser<TToken> : BaseParser<TToken>
    {
        /// <summary>
        /// To temporarily ignore a parser, so it would return a fixed result.        
        /// </summary>
        /// <param name="action"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Parser ignore(Parser action , bool result) { return () => { action(); return result; }; }

        #region Alert functions
        /// <summary>
        /// To see the current token in the console for debugging purposes.
        /// </summary>
        public void alertCurrent() { Console.WriteLine(source.getCurrent()); }

        public Parser alert(string message) { return () => { Console.WriteLine(message); return false; }; }

        public Parser alert(string message , Parser parser) { return any(ignore(alert(message) , true) , parser); }
        #endregion

        #region Monitoring
        int currentIndent = 0;

        protected override void beginSubParsers(IEnumerable<Parser> subParsers)
        {
            base.beginSubParsers(subParsers);
            currentIndent = currentIndent + 1;
        }

        protected override void endSubParsers()
        {
            endSubParsers();
            currentIndent = currentIndent - 1;
        }

        protected override bool invokeParser(Parser parser)
        {
            var description = parser.GetModel().GetFullDefinition();
            Console.WriteLine((currentIndent.ToString().PadLeft(currentIndent , ' ') + "Running parser : " + description).PadRight(currentIndent , ' '));
            Console.ReadLine();
            var result = parser();
            Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent , ' ') + (result ? "Passed" : "Failed"));
            return result;
        }

        protected override bool invokeTokenChecker(TokenChecker<TToken> tokenChecker , TToken token)
        {
            var expectation = tokenChecker.GetExpectedToken();
            if (expectation != null)
                Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent , ' ') + "looking for " + expectation);
            return tokenChecker(token);
        }
        #endregion
    }

    public abstract partial class DebugingTokenParser : TokenParser
    {
        public new Parser expectKeyword_of(string keyword)
        {
            return alert("looking for " + keyword + " keyword" ,
                () => {
                    alertCurrent();
                    var result = base.expectKeyword_of(keyword)();
                    Console.WriteLine(result ? "Keyword Found" : "Keyword Not Found");
                    return result;
                });
        }


        /// <summary>
        /// To temporarily ignore a parser, so it would return a fixed result.        
        /// </summary>
        /// <param name="action"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public Parser ignore(Parser action , bool result) { return () => { action(); return result; }; }

        #region Alert functions
        /// <summary>
        /// To see the current token in the console for debugging purposes.
        /// </summary>
        public void alertCurrent() { Console.WriteLine(source.getCurrent()); }

        public Parser alert(string message) { return () => { Console.WriteLine(message); return false; }; }

        public Parser alert(string message , Parser parser) { return any(ignore(alert(message) , true) , parser); }
        #endregion

        #region Monitoring
        int currentIndent = 0;

        protected override void beginSubParsers(IEnumerable<Parser> subParsers)
        {
            base.beginSubParsers(subParsers);
            currentIndent = currentIndent + 1;
        }

        protected override void endSubParsers()
        {
            endSubParsers();
            currentIndent = currentIndent - 1;
        }

        protected override bool invokeParser(Parser parser)
        {
            var description = parser.GetModel().GetFullDefinition();
            Console.WriteLine((currentIndent.ToString().PadLeft(currentIndent , ' ') + "Running parser : " + description).PadRight(currentIndent , ' '));
            Console.ReadLine();
            var result = parser();
            Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent , ' ') + (result ? "Passed" : "Failed"));
            return result;
        }

        protected override bool invokeTokenChecker(TokenChecker<token> tokenChecker , token token)
        {
            var expectation = tokenChecker.GetExpectedToken();
            if (expectation != null)
                Console.WriteLine(currentIndent.ToString().PadLeft(currentIndent , ' ') + "looking for " + expectation);
            return tokenChecker(token);
        }
        #endregion
    }
}
