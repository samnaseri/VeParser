

using System;
using NUnit.Framework;

namespace VeParser.Test
{
    [TestFixture]
    public class ImplicitFuncToParser_tests : VeParserTests
    {
        [Test]
        public void Test()
        {
            var isletter = (Func<char, bool>)char.IsLetter;
            var isdigit = (Parser<char>)char.IsDigit;
            var input = "input111";
            var parser = V.Seq(V.ZeroOrMore<char>(isletter), V.ZeroOrMore<char>(isdigit));
            var output = runParser(parser, input);
            Assert.NotNull(output);
        }
    }
}
