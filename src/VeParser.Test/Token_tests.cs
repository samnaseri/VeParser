using NUnit.Framework;
using VeParser_vNext;

namespace VeParser.Test
{
    [TestFixture]
    public class Token_tests : VeParserTests
    {
        protected Parser<char> getParserExpectingAn_O()
        {
            return V.Token('O');
        }
        [Test]
        public void V_Token_should_proceed_if_input_is_matched()
        {
            var inputText = "O";
            var parser = getParserExpectingAn_O();
            var output = runParser(parser, inputText);
            Assert.True(output.Success);
            Assert.AreEqual('O', output.Result);
            Assert.AreEqual(1, output.Position);
        }
        [Test]
        public void V_Token_should_not_proceed_if_input_is_not_matching()
        {
            var inputText = "D";
            var parser = getParserExpectingAn_O();
            var output = runParser(parser, inputText);
            Assert.False(output.Success);
            Assert.AreEqual(null, output.Result);
            Assert.AreEqual(0, output.Position);
        }
    }
}
