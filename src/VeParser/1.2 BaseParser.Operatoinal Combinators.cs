using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    public partial class BaseParser<TToken>
    {
        //
        // Parser combinators
        //
        // Thses methods(parsers combinators) are provided for language developers to author their own parser by combining parsers .
        // When combining parsers you will get another parser that its behaviour is based on the given parsers.
        // Parser combinators will receive a list of parsers as their input and then return a parser.
        //
        // For example the any parser checks parsers one by one until find a parser that becomes successfull, if found then
        // parse process will follow that parser, otherwise the any parser will return false as a failur signal.

        /// <summary>
        /// At least one of the specified parsers is expected. The combined parser tries the parsers one by one to find the first one which succeeds.
        /// </summary>
        /// <param name="parsers">The source parsers which can be passed as an array.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser anyParser(IEnumerable<Parser> parsers)
        {
            Parser result = () =>
            {
                BeginSubParsers(parsers); // Notify the parser author that any combinator is begining to invoke the parsers.
                input.SavePosition();
                foreach (var parser in parsers)
                {
                    var xResult = InvokeParser(parser);
                    if (xResult)
                    {
                        input.ConfirmPosition();
                        EndSubParsers();
                        return true;
                    }
                }
                input.RestorePosition();
                EndSubParsers();
                return false;
            };
            result.SetModel("Any of", parsers);
            result.SetActionDescription("Any of ( \n" + string.Join(" , ", parsers.Select(parser => "\n\t" + parser.GetActionDescription()).ToArray()) + "\n)");
            return result;
        }

        /// <summary>
        /// At least one of the specified parsers is expected. The combined parser tries the parsers one by one to find the first one which succeeds.
        /// </summary>
        /// <param name="parsers">The parsers to be checked.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser any(params Parser[] parsers)
        {
            return anyParser(parsers);
        }

        /// <summary>
        /// A sequence of parsers is expected. The combined parser succeeds when all the parsers succeed in the sequence.
        /// </summary>
        /// <param name="parsers">The parsers to be combined.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser seqParser(IEnumerable<Parser> parsers)
        {
            Parser result = () =>
            {
                BeginSubParsers(parsers);
                input.SavePosition();
                foreach (var parser in parsers)
                {
                    var parserResult = InvokeParser(parser);
                    if (!parserResult)
                    {
                        input.RestorePosition();
                        EndSubParsers();
                        return false;
                    }
                }
                input.ConfirmPosition();
                EndSubParsers();
                return true;
            };
            result.SetModel("Sequence of", parsers);
            result.SetActionDescription("Sequence of ( " + string.Join(" , ", parsers.Select(parser => parser.GetActionDescription()).ToArray()));
            return result;
        }

        /// <summary>
        /// A sequence of parsers is expected. The combined parser succeeds when all the parsers succeed in the sequence.
        /// </summary>
        /// <param name="parsers">The parsers to be combined.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser seq(params Parser[] parsers)
        {
            return seqParser(parsers);
        }

        /// <summary>
        /// The specified parser can be ignored or it may happen 1 time or 2 times or more.
        /// </summary>
        /// <param name="parser">The parser to be combined.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser zeroOrMore(Parser parser)
        {
            Parser result = () =>
            {
                while (
                    !input.IsEndOfFile  // dont continue if we are at the end of input string since zero occurrences are ok.
                    && InvokeParser(parser)) ;
                return true;
            };
            result.SetModel("Zero or more of ", parser);
            result.SetActionDescription("Zero or More of ( " + parser.GetActionDescription() + " )");
            return result;
        }

        /// <summary>
        /// The specified parser may happen or not.
        /// </summary>
        /// <param name="parser">The parser to be combined.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser zeroOrOne(Parser parser)
        {
            Parser result = () =>
            {
                if (!input.IsEndOfFile)   // dont continue if we are at the end of input string since zero occurrences are ok.
                    InvokeParser(parser); // it doesn't matter whether the checker function returns true or false
                return true;
            };
            result.SetModel("Zero or one of ", parser);
            result.SetActionDescription("Zero Or One of ( " + parser.GetActionDescription() + " )");
            return result;
        }

        /// <summary>
        /// The specified parser is expected to happen at least one time, but can be repeated more.
        /// </summary>
        /// <param name="parser">The parser to be combined.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser oneOrMore(Parser parser)
        {
            var result = seq(parser, zeroOrMore(parser));
            result.SetModel("One or more of", parser);
            result.SetActionDescription("One or More of ( " + parser.GetActionDescription() + " )");
            return result;
        }

        /// <summary>
        /// Reverses the result of the specified parser.
        /// </summary>
        /// <param name="parser">The parser to be combined.</param>
        /// <returns>The combined result parser.</returns>
        protected Parser not(Parser parser)
        {
            Parser resultParser = () => { var parserResult = InvokeParser(parser); return !parserResult; };
            resultParser.SetModel("Not ", parser);
            return resultParser;
        }

        protected Parser zeroOrMoreAny(params Parser[] list)
        {
            return zeroOrMore(any(list));
        }

        protected Parser zeroOeMoreSeq(params Parser[] list)
        {
            return zeroOrMore(seq(list));
        }

        protected Parser oneOrMoreAny(params Parser[] list)
        {
            return oneOrMore(any(list));
        }

        protected Parser oneOrMoreSeq(params Parser[] list)
        {
            return oneOrMore(seq(list));
        }

        /// <summary>
        /// A list of parser is expected but the delimitParser is expected between each two.
        /// </summary>
        /// <param name="delimitParser">The parser between each two expectation</param>
        /// <param name="listItemParser">The parser for list items.</param>
        /// <returns>The combined parser.</returns>
        protected Parser deleimitedList(Parser delimitParser, Parser listItemParser)
        {
            Parser result =
            seq(
                listItemParser,
                zeroOrMore(
                    seq(
                        delimitParser,
                        listItemParser)));
            result.SetModel("Deleimited list of {0} delimited by {1}", new[] { listItemParser, delimitParser });
            result.SetActionDescription("Delimited List of( " + listItemParser.GetActionDescription() + " )" + " Delimited By (" + delimitParser.GetActionDescription() + " )");
            return result;
        }

        /// <summary>
        /// Any combination of specified parsers is expected. For two parsers it is equal to any(seq(p1,p2),seq(p2,p1)) and for three parsers it is equal to any(seq(p1,p2,p3),seq(p1,p3,p2),seq(p2,p1,p3),seq(p2,p3,p1),seq(p3,p1,p2),seq(p3,p2,p1))
        /// </summary>
        /// <param name="parsers">The parsers to be combined.</param>
        /// <returns>The combined parser.</returns>
        protected Parser mixture(params Parser[] parsers)
        {
            Parser result = null;
            if (parsers.Length == 2)
            {
                result = any(seq(parsers[0], parsers[1]), seq(parsers[1], parsers[0]), parsers[0], parsers[1]);
            }
            else
            {
                result = mixture(parsers[0], mixture(parsers.Skip(1).ToArray()));
            }
            result.SetModel("Mixture of ", parsers);
            result.SetActionDescription("Mixture of ( " + string.Join(" , ", parsers.Select(parser => parser.GetActionDescription()).ToArray()));
            return result;
        }
    }
}