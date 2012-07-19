using NUnit.Framework;


namespace VeParser.Test
{
    [TestFixture]
    public class EOI_tests : VeParserTests
    {
        [Test]
        public void should_accept_an_empty_string_for_EOF_parser()
        {
            var input = "";
            var parser = V.EOI<char>();
            var output = runParser(parser, input);
            Assert.NotNull(output);
        }
        [Test]
        public void should_fail_EOF_parser_on_string_with_content()
        {
            var input = "hey";
            var parser = V.EOI<char>();
            var output = runParser(parser, input);
            Assert.Null(output);
        }
    }
}
