using NUnit.Framework;
using VeParser_vNext;

namespace VeParser.Test
{
    [TestFixture]
    public class OneOrMore_tests : VeParserTests
    {
        [Test]
        public void should_fail_if_zero_occurance_of_token_is_in_input()
        {
            var input = "x";
            var parser = V.OneOrMore(V.Token('a'));
            var output = runParser(parser, input);
            Assert.False(output.Success);
            Assert.AreEqual(0, output.Position);
        }
        [Test]
        public void should_pass_if_one_occurance_of_token_is_in_input()
        {
            var input = "ac";
            var parser = V.OneOrMore(V.Token('a'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_two_occurances_of_token_is_in_input()
        {
            var input = "aa";
            var parser = V.OneOrMore(V.Token('a'));
            var output = runParser(parser, input);
            Assert.True(output.Success);
            Assert.AreEqual(2, output.Position);
        }
    }
}
