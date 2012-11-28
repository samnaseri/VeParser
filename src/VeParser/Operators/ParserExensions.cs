using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VeParser
{
    public static class ParserExensions
    {
        public static Parser<TToken> Then<TToken>(this Parser<TToken> parser, Action<object> onSuccess)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    onSuccess(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> Then<TToken>(this Parser<TToken> parser, Action onSuccess)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    onSuccess();
                }
                return output;
            });
        }
        public static Parser<TToken> OnFail<TToken>(this Parser<TToken> parser, Action onFail)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output == null) {
                    onFail();
                }
                return output;
            });
        }
        public static Parser<TToken> ReplaceOutput<TToken, TNewResult>(this Parser<TToken> parser, Func<object, TNewResult> renderFunc)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    var newResult = renderFunc(output.Result);
                    output = new ParseOutput<TToken>(output.Position, newResult);
                }
                return output;
            });
        }
        public static Parser<char> StringfyOutput(this Parser<char> parser)
        {
            return ReplaceOutput(parser, o => new string(((object[])o).Cast<char>().ToArray()));
        }
        public static Parser<TToken> GetOutput<TToken>(this Parser<TToken> parser, Action<object> outputReceiver)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    outputReceiver(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> PickOutput<TToken>(this Parser<TToken> parser, int index)
        {
            return ReplaceOutput(parser, o => ((object[])o)[index]);
        }
        public static Parser<TToken> AddToList<TToken>(this Parser<TToken> parser, Func<IList> list)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    list().Add(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> AddToList<TToken>(this Parser<TToken> parser, IList list)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    list.Add(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> Append<TToken>(this Parser<TToken> parser, Func<StringBuilder> target)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    target().Append(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> Append<TToken>(this Parser<TToken> parser, StringBuilder target)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    target.Append(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> Append<TToken>(this Parser<TToken> parser, Func<StringBuilder> target, string format)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    target().AppendFormat(format, output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> AppendLine<TToken>(this Parser<TToken> parser, Func<StringBuilder> target)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    target().AppendLine(output.Result.ToString());
                }
                return output;
            });
        }

        public static Parser<TToken> IgnoreOutput<TToken>(this Parser<TToken> parser)
        {
            return ReplaceOutput(parser, output => default(TToken));
        }

        public static Parser<TToken> Star<TToken>(this Parser<TToken> parser)
        {
            return V.ZeroOrMore(parser);
        }
        public static Parser<TToken> Plus<TToken>(this Parser<TToken> parser)
        {
            return V.OneOrMore(parser);
        }
        public static Parser<TToken> Optional<TToken>(this Parser<TToken> parser)
        {
            return V.ZeroOrOne(parser);
        }
        public static Parser<TToken> Repeat<TToken>(this Parser<TToken> parser, int? minRepeatCount, int? maxRepeatCount)
        {
            return V.Repeat(parser, minRepeatCount, maxRepeatCount);
        }

        // Debugging
        public static Parser<TToken> Log<TToken>(this Parser<TToken> parser, string name)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    Console.WriteLine(string.Format("Parser {0} succeeded, position:{1} output:{2}", name, output.Position, output.Result));
                }
                else {
                    Console.WriteLine("Parser " + name + " failed");
                }
                return output;
            });
        }

        // Parsing Strings
        public static ParseOutput<char> Run(this Parser<char> parser, string stringToParse)
        {
            return parser.Run(new SimpleParseContext<char>(new SimpleCharReader(stringToParse)), 0);
        }
    }

    [Serializable]
    class SimpleCharReader : IInput<char>
    {
        string input;
        int length = 0;
        char[] chars;
        public SimpleCharReader(string input)
        {
            chars = input.ToCharArray();
            chars = chars.Concat(new[] { default(char) }).ToArray();
            this.input = input;
            this.length = input.Length;
        }
        public char GetTokenAtPosition(int position)
        {
            return chars[position];
        }
    }

    namespace Predefined
    {
        /// <summary>
        /// Represents a character withing a file.
        /// </summary>
        [System.Diagnostics.DebuggerDisplay("{Character} - {LineNumber},{ColumnNumber}")]
        public class CodeCharacter
        {
            public CodeCharacter(int LineNumber, int ColumnNumber, char character)
            {
                this.LineNumber = LineNumber;
                this.ColumnNumber = ColumnNumber;
                this.Character = character;
            }
            public int ColumnNumber { get; set; }
            public int LineNumber { get; set; }
            public char Character { get; set; }
        }
        [System.Diagnostics.DebuggerDisplay("{LineNumber} - {Indent} - {Characters}")]
        public class IndentedLine
        {
            public IndentedLine()
            {

            }
            public IndentedLine(int lineNumber, int indent, CodeCharacter[] characters)
            {
                this.LineNumber = lineNumber;
                this.Indent = indent;
                this.Characters = characters;
            }
            public CodeCharacter[] Characters { get; set; }
            public int Indent { get; set; }
            public IndentedLine Sublines { get; set; }
            public int LineNumber { get; set; }
        }
        public class SpecialParsers
        {
            public static Parser<char> GetCodeCharactersParser()
            {
                var characters = new List<CodeCharacter>();
                Parser<char> line, character;
                var currentColumnNumber = 0;
                var currentLineNumber = 0;
                Action nextLine = () => { currentLineNumber++; currentColumnNumber = 0; };
                Action nextColumn = () => { currentColumnNumber++; };

                character =
                    C.Except('\n', '\r')
                    .ReplaceOutput(ch => new CodeCharacter(currentLineNumber, currentColumnNumber - 1, (char)ch))
                    .Then(nextColumn)
                    .AddToList(() => characters);

                line = character.Star() + V.Any<char>("\r\n", "\n", "\r");
                line = line.Then(nextLine);

                var file = line.Star().ReplaceOutput(output => characters);
                return file;
            }
            public static Parser<CodeCharacter> GetIndentedLinesParser()
            {
                var lines = new List<IndentedLine>();

                Parser<CodeCharacter> file, line, indent, linecontent;

                var currentLineNumber = 0;
                var indentLength = 0;
                var linecharacters = new List<CodeCharacter>();

                Action goToNextLine = () => { currentLineNumber++; };
                Action incrementIndentLength = () => { indentLength++; };
                Action resetLine = () => { linecharacters = new List<CodeCharacter>(); indentLength = 0; };
                Func<CodeCharacter, bool> stillReadingIndent = c => c.Character == ' ' && c.LineNumber == currentLineNumber;
                Func<CodeCharacter, bool> stillReadingLineContent = c => c.LineNumber == currentLineNumber;
                Action collectLine = () => { lines.Add(new IndentedLine(currentLineNumber, indentLength, linecharacters.ToArray())); };

                indent = V.If(stillReadingIndent).Then(incrementIndentLength).Star();
                linecontent = V.If(stillReadingLineContent).AddToList(() => linecharacters).Star();
                line = (indent + linecontent).Then(collectLine).Then(resetLine).Then(goToNextLine);
                file = line.Star();

                return file.ReplaceOutput(old => lines);
            }
        }

        public static class ParserExtensions
        {
            public static object Run(this Parser<char> parser, string input)
            {
                var output = parser.Run(new SimpleParseContext<char>(new GenericInput<char>(input.ToCharArray())), 0);
                if (output != null)
                    return output.Result;
                return null;
            }
            public static object Run<TToken>(this Parser<TToken> parser, TToken[] tokens)
            {
                var output = parser.Run(new SimpleParseContext<TToken>(new GenericInput<TToken>(tokens)), 0);
                if (output != null)
                    return output.Result;
                return null;
            }
        }

        public class GenericInput<TToken> : IInput<TToken>
        {
            TToken[] tokens;
            public GenericInput(TToken[] array)
            {
                var list = array.ToList();
                list.Add(default(TToken));
                tokens = list.ToArray();
            }

            public TToken GetTokenAtPosition(int position)
            {
                return tokens[position];
            }
        }
    }
}
