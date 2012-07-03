using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        protected Parser @any(IEnumerable<Parser> parsers)
        {
            Parser result = () => {
                beginSubParsers(parsers);
                source.SavePosition();
                foreach (var parser in parsers) {
                    var xResult = invokeParser(parser);
                    if (xResult) {
                        source.ConfirmPosition();
                        endSubParsers();
                        return true;
                    }
                }
                source.RestorePosition();
                endSubParsers();
                return false;
            };
            result.SetModel("Any of" , parsers);
            result.SetActionDescription("Any of ( \n" + string.Join(" , " , parsers.Select(parser => "\n\t" + parser.GetActionDescription()).ToArray()) + "\n)");
            return result;
        }

        protected Parser any(params Parser[] parsers)
        {
            return @any(parsers);
        }

        protected Parser @seq(IEnumerable<Parser> parsers)
        {
            Parser result = () => {
                beginSubParsers(parsers);
                source.SavePosition();
                foreach (var parser in parsers) {
                    var parserResult = invokeParser(parser);
                    if (!parserResult) {
                        source.RestorePosition();
                        endSubParsers();
                        return false;
                    }
                }
                source.ConfirmPosition();
                endSubParsers();
                return true;
            };
            result.SetModel("Sequence of" , parsers);
            result.SetActionDescription("Sequence of ( " + string.Join(" , " , parsers.Select(parser => parser.GetActionDescription()).ToArray()));
            return result;
        }

        protected Parser seq(params Parser[] parsers)
        {
            return @seq(parsers);
        }

        protected Parser zeroOrMore(Parser parser)
        {
            Parser result = () => {
                while (
                    !source.IsEndOfFile  // dont continue if we are at the end of input string since zero occurrences are ok.
                    && invokeParser(parser)) ;
                return true;
            };
            result.SetModel("Zero or more of " , parser);
            result.SetActionDescription("Zero or More of ( " + parser.GetActionDescription() + " )");
            return result;
        }

        protected Parser zeroOrOne(Parser parser)
        {
            Parser result = () => {
                if (!source.IsEndOfFile)   // dont continue if we are at the end of input string since zero occurrences are ok.
                    invokeParser(parser); // it doesn't matter whether the checker function returns true or false
                return true;
            };
            result.SetModel("Zero or one of " , parser);
            result.SetActionDescription("Zero Or One of ( " + parser.GetActionDescription() + " )");
            return result;
        }

        protected Parser oneOrMore(Parser parser)
        {
            var result = seq(parser , zeroOrMore(parser));
            result.SetModel("One or more of" , parser);
            result.SetActionDescription("One or More of ( " + parser.GetActionDescription() + " )");
            return result;
        }

        protected Parser not(Parser parser)
        {
            Parser resultParser = () => { var parserResult = invokeParser(parser); return !parserResult; };
            resultParser.SetModel("Not " , parser);
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

        protected Parser deleimitedList(Parser delimitParser , Parser listItemParser)
        {
            Parser result =
            seq(
                listItemParser ,
                zeroOrMore(
                    seq(
                        delimitParser ,
                        listItemParser)));
            result.SetModel("Deleimited list of {0} delimited by {1}" , new[] { listItemParser , delimitParser });
            result.SetActionDescription("Delimited List of( " + listItemParser.GetActionDescription() + " )" + " Delimited By (" + delimitParser.GetActionDescription() + " )");
            return result;
        }

        protected Parser mixture(params Parser[] parsers)
        {
            Parser result = null;
            if (parsers.Length == 2) {
                result = any(seq(parsers[0] , parsers[1]) , seq(parsers[1] , parsers[0]) , parsers[0] , parsers[1]);
            }
            else {
                result = mixture(parsers[0] , mixture(parsers.Skip(1).ToArray()));
            }
            result.SetModel("Mixture of " , parsers);
            result.SetActionDescription("Mixture of ( " + string.Join(" , " , parsers.Select(parser => parser.GetActionDescription()).ToArray()));
            return result;
        }

        #region uncompleted

        /// <summary>
        /// This method is not available yet.
        /// Parallel variation of any.
        /// </summary>
        /// <param name="parsers"></param>
        /// <returns></returns>
        protected Parser any_p(params Parser[] parsers)
        {
            Parser result = () => {
                int? parentThreadID = Task.CurrentId; // To each thread in the any execution threads, the current thread is their parent
                var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 8 };
                object parse_result_object = null;
                bool parse_result = false;

                Parallel.ForEach(parsers , parallelOptions , (Parser parser , ParallelLoopState state) => {
                    if (parse_result)
                        return;
                    source.StartBranch(parentThreadID);
                    parseTree.BeginBranch();

                    var parserResult = parser();

                    lock (this) {
                        var parse_result_object_temp = parseTree.EndBranch();
                        if (parse_result_object == null)
                            parse_result_object = parse_result_object_temp;

                        if (parserResult) {
                            source.ConfirmBranch(parentThreadID);
                            parse_result = true;
                            state.Break();
                            return;
                        }
                        else {
                            source.IgnoreBranch();
                        }
                    }
                });
                if (parse_result_object != null)
                    parseTree.SetCurrent(parse_result_object);
                return parse_result;
            };
            result.SetModel("Any of(parallel)" , parsers);
            result.SetActionDescription("Parallel Any of (\n " + string.Join(" , " , parsers.Select(parser => "\n\t" + parser.GetActionDescription()).ToArray()) + "\n)");
            return result;
        }

        #endregion uncompleted
    }
}