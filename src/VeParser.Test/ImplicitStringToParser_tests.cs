using System;
using NUnit.Framework;
using VeParser_vNext;
namespace VeParser.Test
{
    [TestFixture]
    public class ImplicitStringToParser_tests : VeParserTests
    {
        [Test]
        public void Test_shoudld_pass()
        {
            var isletter = (Func<char, bool>)char.IsLetter;
            var isdigit = (Func<char, bool>)char.IsDigit;
            var input = "input111";
            var parser = V.Seq<char>("input", "111");
            var output = runParser(parser, input);
            Assert.True(output.Success);
        }

        [Test]
        public void Test_shoudld_fail()
        {
            var isletter = (Func<char, bool>)char.IsLetter;
            var isdigit = (Func<char, bool>)char.IsDigit;
            var input = "input121";
            var parser = V.Seq<char>("input", "111");
            var output = runParser(parser, input);
            Assert.False(output.Success);
        }
    }
}
