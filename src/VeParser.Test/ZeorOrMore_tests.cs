using NUnit.Framework;
using VeParser_vNext;

namespace VeParser.Test
{
    [TestFixture]
    public class ZeorOrMore_tests : VeParserTests
    {
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
