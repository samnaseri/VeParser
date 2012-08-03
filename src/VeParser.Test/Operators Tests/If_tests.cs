﻿// -----------------------------------------------------------------------
// <copyright file="If_tests.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace VeParser.Test
{
    using NUnit.Framework;


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
    public class If_tests : VeParserTests
    {
        [Test]
        public void test1()
        {
            var ws = V.ZeroOrMore<char>(" ");
            var Identitifer = V.OneOrMore<char>("a");
            var ClassName = Identitifer;
            var Members = V.ZeroOrMore(
                                    V.Any(
                                        V.Seq<char>(ws, "public", ws, Identitifer, ws, Identitifer, ws, "{", ws, "get;", ws, "set;", ws, "}"),
                                        V.Seq<char>(ws, Identitifer, ws, Identitifer, ';')
                                    ));
            var classParser = V.Seq("class", ws, ClassName, ws, "{", ws, Members, ws, "}", ws);

            var input = "class aaaa { public aa  aa {get; set; } }";

            var output = runParser(classParser, input);

            Assert.NotNull(output);
        }
    }
}