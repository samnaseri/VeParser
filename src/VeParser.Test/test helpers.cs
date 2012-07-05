using VeParser_vNext;

namespace VeParser.Test
{
    public class VeParserTests
    {
        protected Parser<char> getParserExpectingAn_O()
        {
            return V.Token('O');
        }
        protected ParseOutput<char> runParser(Parser<char> parser, string input)
        {
            var inputReader = new SimpleCharReader(input);
            return parser(new ParseInput<char>(inputReader));
        }
    }

    class SimpleCharReader : IInput<char>
    {
        string input;
        int length = 0;
        public SimpleCharReader(string input)
        {
            this.input = input;
            this.length = input.Length;
        }
        public char GetTokenAtPosition(int position)
        {
            if (position >= length)
                return default(char);
            return input[position];
        }
    }
}
