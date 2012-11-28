using System;
using System.Linq;

namespace VeParser.Test
{
    public class VeParserTests
    {
        protected ParseOutput<char> runParser(Parser<char> parser, string input)
        {
            var inputReader = new SimpleCharReader(input);
            return parser.Run(new SimpleParseContext<char>(inputReader), 0);
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
}
