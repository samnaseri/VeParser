using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace VeParser.Test
{
    [TestFixture]
    public class URLParsing_tests : VeParserTests
    {
        [Test]
        public void TestUsingRegex()
        {
            var input = "http://media.jsonline.com/designimages/";

            var pattern = @"(?<scheme>(https?|ftp):/?/?)?(?<host>[^:/\s]+)(?<path>((?:/\w+)*)/)(?<file>[-\w.]+[^#?\s]*)?";
            var regex = new Regex(pattern, RegexOptions.Compiled);
            var optional = C.ZeroOrOne;
            var seq = C.Seq;
            var any = C.Any;
            var star = C.ZeroOrMore;
            var plus = C.OneOrMore;
            var times = C.Repeat;
            var not = C.Not;

            var parser =
                V.TakeUntil(':', ((Parser<char>)"https" | "http" | "ftp")) + ':' + V.Repeat<char>('/', 0, 2) +
                plus(C.Except('/', ':', ' ', '\t', '\r', '\n')) +
                star('/' + plus(C.LetterOrDigit)) + '/' + V.EOI<char>();

            //parser = V.TakeUntil(':', V.Any<char>("https", "", "")) + V.Repeat('/',0,2) + ;

            Stopwatch watch;
            var count = 10000;
            watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < count; i++) {
                var o = runParser(parser, input);
                Assert.NotNull(o);
            }
            watch.Stop();
            var VeParserTime = watch.ElapsedMilliseconds;

            watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < count * 3; i++) {
                var matches = regex.Matches(input);
                var matchRealized = matches.Cast<Match>().ToArray();
            }
            watch.Stop();
            var regExTime = watch.ElapsedMilliseconds;
            var diff = regExTime - VeParserTime;
            Assert.True(VeParserTime < regExTime);
            Assert.True(diff > 0);
        }
    }
}
