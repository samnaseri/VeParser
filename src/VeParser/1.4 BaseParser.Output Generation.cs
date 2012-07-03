using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;

namespace VeParser
{

    public partial class BaseParser<TToken>
    {
        // PURPOSE
        // To generate output while parsing the content.
        //

        private Stack output;

        #region output generation control methods
        protected dynamic CreateDynamicObject()
        {
            return new ExpandoObject();
        }

        protected dynamic CreateDynamicListObject()
        {
            return new List<dynamic>();
        }

        protected void InitializeOutput(object output)
        {
        }

        protected void FinalizeOutput(object output)
        {

        }

        private static object[] dummyObjectArray = new object[] { };
        protected void SetFieldOrPropertyValue(object target, string propertyName, object value)
        {
            if (target is string)
            {
                string pattern = (string)target;
                var updatedOutput = pattern.Replace(propertyName, value == null ? "" : value.ToString());
                output.Pop(); // Since the reference itself updated we need to pop the old reference and push the new reference.
                output.Push(updatedOutput);
            }
            var dynamicTarget = target as IDictionary<string, object>;
            if (dynamicTarget != null)
            {
                //dynamic dynamicTarget2 = target;
                dynamicTarget[propertyName] = value;
            }
            else
            {
                // TODO : we may cache the result for the following line, 
                // something like this : 
                // if ( cachedMemberInfo(targetType,propertyName)) { 
                //      cachedMemberInfo(targetType,propertyName)(value); 
                // } else { 
                //      var member = findMember(obj, prop);
                //      var fieldInfo = member as FieldInfo;
                //      var propertyInfo = member as PropertyInfo;
                //      if ( fieldInfo != null ) setChachedMemberInfo(targetType,propertyName,(obj,prop)=>{ fieldInfo.SetValue(obj,prop); });
                //      if ( propertyInfo != null ) setChachedMemberInfo(targetType,propertyName,(obj,prop)=>{ propertyInfo.SetValue(obj,prop, dummyObjectArray); });
                //}
                var members = target.GetType().GetMember(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var member in members)
                {
                    var field = member as FieldInfo;
                    if (field != null)
                        field.SetValue(target, value);
                    var property = member as PropertyInfo;
                    if (property != null)
                        property.SetValue(target, value, dummyObjectArray); // TODO : What happens if I pass null instead of dummyObjectArray for index??
                    if (field != null || property != null)
                        return;
                }
            }
        }
        protected object GetFieldOrPropertyValue(object target, string propertyName)
        {
            var dynamicTarget = target as IDictionary<string, object>;
            if (dynamicTarget != null)
            {
                //dynamic dynamicTarget2 = target;
                return dynamicTarget[propertyName];
            }
            else
            {
                // TODO : we may cache the result for the following line, 
                // something like this : 
                // if ( cachedMemberInfo(targetType,propertyName)) { 
                //      cachedMemberInfo(targetType,propertyName)(value); 
                // } else { 
                //      var member = findMember(obj, prop);
                //      var fieldInfo = member as FieldInfo;
                //      var propertyInfo = member as PropertyInfo;
                //      if ( fieldInfo != null ) setChachedMemberInfo(targetType,propertyName,(obj,prop)=>{ fieldInfo.SetValue(obj,prop); });
                //      if ( propertyInfo != null ) setChachedMemberInfo(targetType,propertyName,(obj,prop)=>{ propertyInfo.SetValue(obj,prop, dummyObjectArray); });
                //}
                var members = target.GetType().GetMember(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (var member in members)
                {
                    var field = member as FieldInfo;
                    if (field != null)
                        return field.GetValue(target);
                    var property = member as PropertyInfo;
                    if (property != null)
                        return property.GetValue(target, dummyObjectArray); // TODO : What happens if I pass null instead of dummyObjectArray for index??
                }
                return null;
            }
        }
        protected void AddValueToList(object target, string fieldName, object value)
        {
            dynamic dynamicTarget = target;
            if (!string.IsNullOrEmpty(fieldName))
            {
                if (dynamicTarget[fieldName] != null)
                    dynamicTarget[fieldName] = CreateDynamicListObject();
                dynamicTarget[fieldName].Add(value);
            }
            else
            {
                dynamicTarget.Add(value);
            }
        }
        #endregion

        #region create methods
        protected Parser createNew(Func<object> creator, Parser parser)
        {
            Parser resultParser = () =>
            {
                var newOutputObject = creator();
                InitializeOutput(newOutputObject);
                output.Push(newOutputObject);
                var parserResult = InvokeParser(parser);
                if (!parserResult)
                    output.Pop();
                return parserResult;
            };
            return resultParser;
        }

        protected Parser createNew(Parser parser)
        {
            var resultParser = createNew(() => CreateDynamicObject(), parser);
            resultParser.SetModel("Create output by ", parser);
            return resultParser;
        }

        protected Parser createNewText(string pattern, Parser parser)
        {
            var resultParser = createNew(() => pattern, parser);
            resultParser.SetModel("Add " + pattern + "pattern populated by ", parser);
            return resultParser;
        }

        protected Parser createNewText(Parser parser)
        {
            return createNewText("", parser);
        }

        protected Parser createNew<TDOM>(Parser parser) where TDOM : new()
        {
            var resultParser = createNew(() => new TDOM(), parser);
            resultParser.SetModel("Create a new {" + typeof(TDOM).Name + "} by ", parser);
            return resultParser;
        }

        protected Parser createNewList(Parser parser)
        {
            var resultParser = createNew(() => CreateDynamicListObject(), parser);
            resultParser.SetModel("Create output list by ", parser);
            return resultParser;
        }

        protected Parser createNewList<TDOM>(Parser parser) where TDOM : new()
        {
            var resultParser = createNew(() => new List<TDOM>(), parser);
            resultParser.SetModel("Create a new list of {" + typeof(TDOM).Name + "} by ", parser);
            return resultParser;
        }

        protected Parser create(object newCurrentOutput, Parser parser)
        {
            return createNew(() => newCurrentOutput, parser);
        }
        #endregion

        #region output manipulation general purpose functions
        protected Parser consume(UpdateFunc updateFunc, Parser parser)
        {
            Parser resultParser = () =>
            {
                var parserResult = InvokeParser(parser);
                if (parserResult == true)
                {
                    var parserOutput = output.Pop();
                    FinalizeOutput(parserOutput);
                    var outputTarget = output.Pop();
                    var updatedOutput = updateFunc(outputTarget, parserOutput);
                    output.Push(updatedOutput);
                }
                return parserResult;
            };
            return resultParser;
        }
        protected Parser consume(ModifyFunc outputRecieveFunc, Parser parser)
        {
            return consume((currentOutput, result) => { outputRecieveFunc(currentOutput, result); return currentOutput; }, parser);
            // The following code may have better performance, because it does not pop and push the value if the actual object reference is not changed.
            //Parser resultParser = () =>
            //{
            //    var parserResult = parser();
            //    var parserOutput = output.Pop();
            //    if (parserResult == true)
            //    {
            //        FinalizeOutput(parserOutput);
            //        var outputTarget = output.Peek();
            //        outputRecieveFunc(outputTarget, parserOutput);
            //    }
            //    return parserResult;
            //};
            //return resultParser;
        }

        protected Parser update(ReplaceFunc replaceFunc, Parser parser)
        {
            Parser resultParser = () =>
            {
                var parserResult = InvokeParser(parser);
                if (parserResult == true)
                {
                    var parserOutput = output.Pop();
                    var valueToReplace = replaceFunc(parserOutput);
                    output.Push(valueToReplace);
                }
                return parserResult;
            };
            return resultParser;
        }
        protected Parser update(ReadFunc outputRecieveFunc, Parser parser)
        {
            return update(currentOutput => { outputRecieveFunc(currentOutput); return currentOutput; }, parser);
            //Parser resultParser = () =>
            //{
            //    var parserResult = parser();
            //    if (parserResult == true)
            //    {
            //        var parserOutput = output.Peek();
            //        outputRecieveFunc(parserOutput);
            //    }
            //    return parserResult;
            //};
            //return resultParser;
        }
        #endregion

        protected Parser run(Action action, Parser parser)
        {
            Parser resultParser = () =>
            {
                var parserResult = InvokeParser(parser);
                if (parserResult)
                    action();
                return parserResult;
            };
            return resultParser;
        }

        /// <summary>
        /// Keeps the input value for furhter use in update or consume.
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected Parser keep(Parser parser)
        {
            Parser resultParser = () =>
            {
                output.Push(this.input.getCurrent());
                var parserResult = InvokeParser(parser);
                if (!parserResult) // This if block works based on the fact that generator parsers will push a result on output stack if they succeed and will not push a result if they fail.
                    output.Pop();
                return parserResult;
            };
            return resultParser;
        }

        #region output manipulation functions
        protected Parser add(string fieldName, Transformer transformer, Parser parser)
        {
            ModifyFunc addParserResultToCurrentOutput = (currentOutput, parserResult) => AddValueToList(currentOutput, fieldName, parserResult);
            Parser resultParser = consume(addParserResultToCurrentOutput, parser);
            return resultParser;
        }
        protected Parser add(string fieldName, Parser parser)
        {
            return add(fieldName, result => result, parser);
        }
        protected Parser add(Transformer transformer, Parser parser)
        {
            return add("", transformer, parser);
        }
        protected Parser add(Parser parser)
        {
            return add("", parser);
        }

        protected Parser set(string fieldName, Transformer transformer, Parser parser)
        {
            ModifyFunc setParserResultIntoAPropertyOfCurrentOutput = (currentOutput, parserResult) => SetFieldOrPropertyValue(currentOutput, fieldName, transformer(parserResult));
            var resultParser = consume(setParserResultIntoAPropertyOfCurrentOutput, parser);
            return resultParser;
        }
        protected Parser set(string fieldName, Parser parser)
        {
            return set(fieldName, result => result, parser);
        }

        protected Parser setValue(string fieldName, object value, Parser parser)
        {
            ReadFunc setAPropertyOfCurrentOutputToDesiredValue = (currentOutput) => SetFieldOrPropertyValue(currentOutput, fieldName, value);
            var resultParser = update(setAPropertyOfCurrentOutputToDesiredValue, parser);
            return resultParser;
        }
        protected Parser setTrue(string fieldName, Parser parser)
        {
            return setValue(fieldName, true, parser);
        }
        protected Parser setFalse(string fieldName, Parser parser)
        {
            return setValue(fieldName, false, parser);
        }

        protected Parser appendText(string fieldName, Parser parser)
        {
            return consume(
                 (currentOutput, parserResult) =>
                 {
                     var currentTextValue = GetFieldOrPropertyValue(currentOutput, fieldName);
                     currentTextValue = currentTextValue + parserResult.ToString();
                     SetFieldOrPropertyValue(currentOutput, fieldName, currentTextValue);
                 }, parser
             );
        }
        protected Parser appendText(Parser parser)
        {
            return consume((currentOutput, parserResult) => currentOutput.ToString() + parserResult.ToString(), parser);
        }

        protected Parser replaceText(string targetIndicator, Parser parser)
        {
            return consume((currentOutput, parserResult) => currentOutput.ToString().Replace(targetIndicator, parserResult.ToString()), parser);
        }
        #endregion

    }

    public class ParseTree
    {
        Stack<object> parseStack = new Stack<object>();

        public void Push(object newCurrentValue)
        {
            parseStack.Push(item: newCurrentValue);
        }

        public object Peek()
        {
            return parseStack.Peek();
        }

        public object Pop()
        {
            return parseStack.Pop();
        }

    }
}