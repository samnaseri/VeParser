// -----------------------------------------------------------------------
// <copyright file="If_tests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace VeParser.Test
{
    using NUnit.Framework;
    using VeParser_vNext;

    public class Class
    {
        public string ElementType { get; set; }
        public Parser<char> ClassName { get; set; }
        public Parser<char> Members { get; set; }
    }
    public class Member
    {
        public string Type { get; set; }
    }

    [TestFixture]
    public class If_tests
    {
        [Test]
        public void test1()
        {
            var ws = V.ZeroOrMore<char>(" ");
            var Identitifer = V.OneOrMore<char>("a");

            var classParser = V.Scope(
                new
                {
                    ElementType = "Class",
                    ClassName = Identitifer,
                    Members = V.ZeroOrMore(
                                    V.Any(
                                        V.Scope(new Member { Type = "Auto Property" }, p => V.Seq<char>(ws, "public", ws, Identitifer, ws, Identitifer, ws, "{", ws, "get;", ws, "set;", ws, "}")),
                                        V.Scope(new { Type = "Field" }, p => V.Seq<char>(ws, Identitifer, ws, Identitifer, ";"))
                                    )),
                },
                m => V.Seq("class", ws, m.ClassName, ws, "{", ws, m.Members, ws, "}", ws));

            var input = "class aaaa { public aa  aa {get; set; } }";
            var output = classParser.Run(new ParseInput<char>(new SimpleCharReader(input)));

            Assert.NotNull(output);
        }
    }
}
