using NUnit.Framework;


namespace VeParser.Test
{
    [TestFixture]
    public class Seq_tests : VeParserTests
    {
        [Test]
        public void should_accept_two_tokens_in_sequence_if_input_matches()
        {
            var input = "12";
            var parser = V.Seq(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.AreEqual(new[] { '1', '2' }, output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(2, output.Position);
        }
        [Test]
        public void should_fail_two_tokens_in_sequence_if_input_does_not_match()
        {
            var input = "21";
            var parser = V.Seq(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.Null(output);
        }
    }
}