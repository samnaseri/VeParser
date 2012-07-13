namespace VeParser.Test
{
    using NUnit.Framework;
    using VeParser_vNext;

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
            Assert.True(output.Success);
            Assert.AreEqual(input.Length, output.Position);
        }
        [Test]
        public void Should_fail_with_invalid_input()
        {
            var input = ",12,1212";
            var numberParser = V.Seq(V.Token('1'), V.Token('2'));
            var parser = V.DelimitedList(numberParser, V.Token(','), false);
            var output = runParser(parser, input);
            Assert.AreEqual(null, output.Result);
            Assert.False(output.Success);
            Assert.AreEqual(0, output.Position);
        }
    }
}
