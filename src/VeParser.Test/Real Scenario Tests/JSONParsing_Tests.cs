using NUnit.Framework;
namespace VeParser.Test.Real_Scenario_Tests
{
    [TestFixture]
    public class JSONParsing_Tests : VeParserTests
    {
        public Parser<char> GetJsonParser()
        {
            Parser<char> String, DecimalNumber, IntegerNumber, Number, FieldName, Property, Object, Skip;

            var Value = V.TemporaryReference<char>();
            Skip = C.WhiteSpace.Star();
            String = '\"' + Skip + C.CaptureText(C.Except('\"').Star()) + Skip + '\"';
            DecimalNumber = C.Digit.Star() + '.' + C.Digit.Plus();
            IntegerNumber = C.Digit.Star();
            Number = DecimalNumber | IntegerNumber;
            FieldName = C.CaptureText(C.Letter + C.LetterOrDigit.Star());
            Property = V.Scope(
                dic => new { FieldName = V.ScopeParser(dic, "FieldName", FieldName), Value = V.ScopeParser(dic, "Value", Value) },
                p => p.FieldName + Skip + ':' + Skip + p.Value,
                dic => new { FieldName = dic["FieldName"] as string, Value = dic["Value"] });
            Object = '{' + Skip + V.DelimitedList(Property, ',', true) + Skip + '}';

            Value.SetParser(String | Number | Object);

            return Object;
        }

        [Test]
        public void Test1()
        {
            var input = @"{name:""Sam""}";
            var output = runParser(GetJsonParser(), input);
            Assert.NotNull(output);
        }
    }
}
