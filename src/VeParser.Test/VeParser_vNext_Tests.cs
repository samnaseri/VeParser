
using NUnit.Framework;
using VeParser_vNext;
namespace VeParser.Test
{
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

    [TestFixture]
    public class VeParser_vNext_Tests
    {
        private Parser<char> getParserExpectingAn_O() { return V.Token('O'); }
        private ParseOutput<char> runParser(Parser<char> parser, string input)
        {
            var inputReader = new SimpleCharReader(input);
            return parser(new ParseInput<char>(inputReader));
        }
        [Test]
        public void V_Token_should_proceed_if_input_is_matched()
        {
            var inputText = "O";
            var parser = getParserExpectingAn_O();
            var output = runParser(parser, inputText);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void V_Token_should_not_proceed_if_input_is_not_matching()
        {
            var inputText = "D";
            var parser = getParserExpectingAn_O();
            var output = runParser(parser, inputText);
            Assert.False(output.Success);
            Assert.AreEqual(0, output.Position);
        }
        [Test]
        public void should_accept_an_empty_string_for_EOF_parser()
        {
            var input = "";
            var parser = V.EOI<char>();
            var output = runParser(parser, input);
            Assert.True(output.Success);
        }
        [Test]
        public void should_fail_EOF_parser_on_string_with_content()
        {
            var input = "hey";
            var parser = V.EOI<char>();
            var output = runParser(parser, input);
            Assert.False(output.Success);
        }
        [Test]
        public void should_accept_two_tokens_in_sequence_if_input_matches()
        {
            var input = "12";
            var parser = V.Seq(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(2, output.Position);
        }
        [Test]
        public void should_fail_two_tokens_in_sequence_if_input_does_not_match()
        {
            var input = "21";
            var parser = V.Seq(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.False(output.Success);
            Assert.AreEqual(0, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_1()
        {
            var input = "1";
            var parser = V.Any(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_2()
        {
            var input = "2";
            var parser = V.Any(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_3()
        {
            var input = "3";
            var parser = V.Any(V.Token('1'), V.Token('2'), V.Token('3'), V.Token('4'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_4()
        {
            var input = "2";
            var parser = V.Any(V.Token('1'), V.Token('2'), V.Token('2'), V.Token('4'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_fail_if_none_of_expected_tokens_are_not_in_input()
        {
            var input = "3";
            var parser = V.Any(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.False(output.Success);
            Assert.AreEqual(0, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_sequences_are_in_input_1()
        {
            var input = "ab";
            var parser = V.Any(V.Seq(V.Token('c'), V.Token('d')), V.Seq(V.Token('a'), V.Token('b')));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(2, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_sequences_are_in_input_2()
        {
            var input = "ab";
            var parser = V.Any(V.Seq(V.Token('a'), V.Token('d')), V.Seq(V.Token('a'), V.Token('b')));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(2, output.Position);
        }
        [Test]
        public void should_fail_if_none_of_expected_sequences_are_in_input()
        {
            var input = "eg";
            var parser = V.Any(V.Seq(V.Token('c'), V.Token('d')), V.Seq(V.Token('a'), V.Token('b')));
            var output = runParser(parser, input);
            Assert.False(output.Success);
            Assert.AreEqual(0, output.Position);
        }
        [Test]
        public void should_pass_if_zero_occurances_of_token_is_in_input()
        {
            var input = "";
            var parser = V.ZeroOrMore(V.Token('a'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(0, output.Position);
        }
        [Test]
        public void should_pass_if_one_occurances_of_token_is_in_input()
        {
            var input = "a";
            var parser = V.ZeroOrMore(V.Token('a'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_more_occurances_of_token_is_in_input()
        {
            var input = "aaaaa";
            var parser = V.ZeroOrMore(V.Token('a'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(5, output.Position);
        }
    }
}
