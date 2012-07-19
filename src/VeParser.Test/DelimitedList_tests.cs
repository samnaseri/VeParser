namespace VeParser.Test
{
    using NUnit.Framework;


    [TestFixture]
    public class DelimitedList_tests : VeParserTests
    {
        [Test]
        public void Should_pass_with_valid_input()
        {
            var input = "12,12,12,12";
            var numberParser = V.Seq(V.Token('1'), V.Token('2'));
            var parser = V.DelimitedList(numberParser, V.Token(','), false);
            var output = runParser(parser, input);
            Assert.AreEqual(new[] { new[] { '1', '2' }, new[] { '1', '2' }, new[] { '1', '2' }, new[] { '1', '2' } }, output.Result);
            Assert.NotNull(output);
            Assert.AreEqual(input.Length, output.Position);
        }
        [Test]
        public void Should_fail_with_invalid_input()
        {
            var input = ",12,1212";
            var numberParser = V.Seq(V.Token('1'), V.Token('2'));
            var parser = V.DelimitedList(numberParser, V.Token(','), false);
            var output = runParser(parser, input);
            Assert.Null(output);
        }
    }
}
