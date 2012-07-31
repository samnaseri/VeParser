using NUnit.Framework;

namespace VeParser.Test
{
    [TestFixture]
    public class C_tests : VeParserTests
    {
        [Test]
        public void Test1()
        {
            var input = "input111";

            var parser = V.Seq(V.ZeroOrMore(C.Letter), V.ZeroOrMore(C.Digit));
            var output = runParser(parser, input);
            Assert.NotNull(output);
        }
    }
}
