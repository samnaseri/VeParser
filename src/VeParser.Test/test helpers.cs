using System;
using System.Linq;

namespace VeParser.Test
{
    public class VeParserTests
    {
        protected ParseOutput<char> runParser(Parser<char> parser, string input)
        {
            var inputReader = new SimpleCharReader(input);
            return parser.Run(new ParseContext<char>(inputReader), 0);
        }
    }

    [Serializable]
    class SimpleCharReader : IInput<char>
    {
        string input;
        ushort length = 0;
        char[] chars;
        public SimpleCharReader(string input)
        {
            chars = input.ToCharArray();
            chars = chars.Concat(new[] { default(char) }).ToArray();
            this.input = input;
            this.length = (ushort)input.Length;
        }
        public char GetTokenAtPosition(ushort position)
        {
            return chars[position];
        }
    }
}
