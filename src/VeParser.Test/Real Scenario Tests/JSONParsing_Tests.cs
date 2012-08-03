using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace VeParser.Test.Real_Scenario_Tests
{
    [TestFixture]
    public class JSONParsing_Tests : VeParserTests
    {
        [System.Diagnostics.DebuggerDisplay("{Name} : {Value}")]
        public class JSONProp
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public override string ToString()
            {
                return Name + "=" + Value;
            }
        }

        public class JSONObject
        {
            public JSONObject()
            {
                Properties = new List<JSONProp>();
            }
            public List<JSONProp> Properties { get; set; }

            public override string ToString()
            {
                return "\n\t(" + String.Concat(Properties.Select(x => "\n\t" + x.ToString().Replace("\n\t", "\n\t\t"))) + "\n\t)";
            }
        }
        public Parser<char> GetJsonParser()
        {
            Parser<char> String, DecimalNumber, IntegerNumber, Number, FieldName, Property, Object, Skip;

            Skip = C.WhiteSpace.Star();

            var Value = V.TemporaryReference<char>();
            string currentFieldName = null;
            object currentValue = null;


            // Parsing string values
            StringBuilder currentStringBuilder = new StringBuilder();
            String = V
                .StartWith<char>(() => currentStringBuilder.Clear())
                .When('\"' + C.Except('\"').Append(currentStringBuilder).Star() + '\"')
                .Then(() => currentValue = currentStringBuilder.ToString());

            // Parsing numberic values
            int digitIndex = 0, decimalIndex = 0; double currentNumber = 0;
            Action<object> collectDigitNumber = c => { currentNumber = currentNumber * digitIndex + char.GetNumericValue((char)c); digitIndex *= 10; };
            Action<object> collectDecimalNumber = c => { currentNumber += char.GetNumericValue((char)c) / (decimalIndex); decimalIndex *= 10; };
            IntegerNumber = C.Digit.Plus();
            DecimalNumber = C.Digit.Then(collectDigitNumber).Star() + '.' + C.Digit.Then(collectDecimalNumber).Plus();
            Number = V
                .StartWith<char>(() => { currentNumber = 0; digitIndex = 1; decimalIndex = 10; })
                .When(DecimalNumber | IntegerNumber)
                .Then(() => currentValue = currentNumber);

            // Parsing field name
            var currentFieldNameBuilder = new StringBuilder();
            FieldName = C.Letter.Append(currentFieldNameBuilder) + C.LetterOrDigit.Append(currentFieldNameBuilder).Star();
            FieldName = FieldName.Then(() => currentFieldName = currentFieldNameBuilder.ToString());
            FieldName = FieldName | String.Then(() => currentFieldName = (string)currentValue);
            FieldName = V.StartWith<char>(() => currentFieldNameBuilder.Clear()).When(FieldName);


            Stack<JSONObject> objectsStack = new Stack<JSONObject>();
            Stack<JSONProp> propertiesStack = new Stack<JSONProp>();

            //JSONProp currentProperty = null;
            Action collectPropertyName = () => propertiesStack.Peek().Name = currentFieldName;
            Action collectPropertyValue = () => propertiesStack.Peek().Value = currentValue;
            Property = FieldName.Then(collectPropertyName) + Skip + ':' + Skip + Value.Then(collectPropertyValue);
            Property = Property.Then(() => objectsStack.Peek().Properties.Add(propertiesStack.Peek()));
            Property = V
                .StartStack(propertiesStack, () => new JSONProp())
                .When(Property);



            Object = '{' + Skip + V.DelimitedList(Property, Skip + ',' + Skip, true) + Skip + '}';
            Object = Object.Then(() => currentValue = objectsStack.Peek());
            Object = V.StartStack(objectsStack, () => new JSONObject()).When(Object);

            Value.SetParser(String | Number | Object);

            return Object.ReplaceOutput(o => currentValue);
        }

        [Test]
        public void Test1()
        {
            var input = @"{address : {city: ""Melbourne"", Street : ""Lygon""},ContactInfo : { number1 : ""84378437843789""}, name:""Sam"", ""last name"":""naseri"" , age : 28, score : 55.824 }";
            var output = runParser(GetJsonParser(), input);
            Assert.NotNull(output);
        }
    }
}
