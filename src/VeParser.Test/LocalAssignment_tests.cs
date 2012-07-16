
using NUnit.Framework;
namespace VeParser.Test
{
    [TestFixture]
    public class LocalAssignment_tests : VeParserTests
    {
        [Test]
        public void Test()
        {
            var input = "test_meysam";
            var any = C.Any;
            var seq = C.Seq;
            var optional = C.ZeroOrOne;
            var parser = seq(optional("test"), any("_sam", "_meysam"));
            var output = runParser(parser, input);
            Assert.True(output.Success);
        }
    }
}
