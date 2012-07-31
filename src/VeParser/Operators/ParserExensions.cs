using System;
using System.Collections;
using System.Collections.Generic;

namespace VeParser
{
    public static class ParserExensions
    {
        public static Parser<TToken> OnSuccess<TToken>(this Parser<TToken> parser, Action<object> onSuccess)
        {
            return new Parser<TToken>((context, position) => {
                var output = parser.Run(context, position);
                if (output != null) {
                    onSuccess(output.Result);
                }
                return output;
            });
        }
        public static Parser<TToken> OnSuccess<TToken>(this Parser<TToken> parser, Action onSuccess)
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
    }

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
    public class SpecialParsers
    {
        public static Parser<char> GetCodeCharactersParsers()
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
                .OnSuccess(nextColumn)
                .AddToList(characters);

            line = character.Star() + V.Any<char>("\r\n", "\n", "\r");
            line = line.OnSuccess(nextLine);

            var file = line.Star().ReplaceOutput(output => characters);
            return file;
        }
    }
}
