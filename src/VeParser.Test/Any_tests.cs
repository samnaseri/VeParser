using NUnit.Framework;


namespace VeParser.Test
{
    [TestFixture]
    public class Any_tests : VeParserTests
    {
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_1()
        {
            var input = "1";
            var parser = V.Any(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.AreEqual('1', output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_2()
        {
            var input = "2";
            var parser = V.Any(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.AreEqual('2', output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_3()
        {
            var input = "3";
            var parser = V.Any(V.Token('1'), V.Token('2'), V.Token('3'), V.Token('4'));
            var output = runParser(parser, input);
            Assert.AreEqual('3', output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_tokens_is_in_input_4()
        {
            var input = "2";
            var parser = V.Any(V.Token('1'), V.Token('2'), V.Token('2'), V.Token('4'));
            var output = runParser(parser, input);
            Assert.AreEqual('2', output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void should_fail_if_none_of_expected_tokens_are_not_in_input()
        {
            var input = "3";
            var parser = V.Any(V.Token('1'), V.Token('2'));
            var output = runParser(parser, input);
            Assert.Null(output);
        }
        [Test]
        public void should_pass_if_any_of_expected_sequences_are_in_input_1()
        {
            var input = "ab";
            var parser = V.Any(V.Seq(V.Token('c'), V.Token('d')), V.Seq(V.Token('a'), V.Token('b')));
            var output = runParser(parser, input);
            Assert.AreEqual(new[] { 'a', 'b' }, output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(2, output.Position);
        }
        [Test]
        public void should_pass_if_any_of_expected_sequences_are_in_input_2()
        {
            var input = "ab";
            var parser = V.Any(V.Seq(V.Token('a'), V.Token('d')), V.Seq(V.Token('a'), V.Token('b')));
            var output = runParser(parser, input);
            Assert.AreEqual(new[] { 'a', 'b' }, output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(2, output.Position);
        }
        [Test]
        public void should_fail_if_none_of_expected_sequences_are_in_input()
        {
            var input = "eg";
            var parser = V.Any(V.Seq(V.Token('c'), V.Token('d')), V.Seq(V.Token('a'), V.Token('b')));
            var output = runParser(parser, input);
            Assert.Null(output);
        }
    }
}
