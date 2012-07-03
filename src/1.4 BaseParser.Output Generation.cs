using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace VeParser
{
    public partial class BaseParser<TToken>
    {
        public ParseTree parseTree;

        protected Parser create<TDOM>(Parser parser) where TDOM : new()
        {
            Parser result = () => {
                parseTree.SetCurrent(new TDOM());
                var parser_result = invokeParser(parser);
                if (!parser_result)
                    parseTree.Pop();
                return parser_result;
            };
            result.SetModel("Create a new {" + typeof(TDOM).Name + "} by " , parser);
            result.SetActionDescription("Create a new {" + typeof(TDOM).Name + "} By Parser ( " + parser.GetActionDescription() + " )");
            return result;
        }

        protected Parser fill(string fieldName , TokenChecker<TToken> tokenChecker)
        {
            Parser result = to_parser(
                tokenChecker ,
                (token) => { parseTree.FeedCurrent(fieldName , token); });
            result.SetModel("set current." + fieldName + " <= " + tokenChecker.GetExpectedToken());
            result.SetActionDescription("Current." + fieldName + " <= " + tokenChecker.GetExpectedToken());
            return result;
        }

        /// <summary>
        /// This function assumes that an inner call to create method created an object and set as current value.
        /// So this function will loads that current value as property value and sets current value as the previous current value and then sets the specified property's value by loade property value.
        /// The inner parser should contain a call to create method, which will create an object and set as current object. So when this fill method is called.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="parser"></param>
        /// <returns></returns>
        protected Parser fill(string fieldName , Parser parser)
        {
            Parser result = () => {
                var parser_result = parser();
                if (parser_result) {
                    var top = parseTree.Pop();
                    parseTree.FeedCurrent(fieldName , top);
                }
                return parser_result;
            };
            result.SetModel("set current." + fieldName + " <= parser result of " , parser);
            result.SetActionDescription("Current." + fieldName + " <= result of " + parser.GetActionDescription());
            return result;
        }

        protected Parser fill<T , TProp>(Expression<Func<T , TProp>> propertySpecifier , Parser parser)
        {
            var memberAccessExpression = propertySpecifier.Body as MemberExpression;
            var property = memberAccessExpression.Member.Name;
            return fill(property , parser);
        }

        protected Parser fillByTrue(string fieldName , Parser parser)
        {
            return fillByValue(fieldName , true , parser);
        }

        protected Parser fillByValue(string fieldName , object value , Parser parser)
        {
            Parser result = () => { var parser_result = parser(); if (parser_result) parseTree.FeedCurrent(fieldName , value); return parser_result; };
            result.SetModel("if parsed then set current." + fieldName + " <= " + value , parser);
            result.SetActionDescription("if ( " + parser.GetActionDescription() + " ) then Current." + fieldName + " <= " + value);
            return result;
        }
    }

    public class ParseTree
    {
        Stack<object> parseStack = new Stack<object>();
        ConcurrentDictionary<int, Stack<object>> threadStacks = new ConcurrentDictionary<int , Stack<object>>();

        public void SetCurrent(object newCurrentValue)
        {
            if (Task.CurrentId == null)
                parseStack.Push(item: newCurrentValue);
            else {
                threadStacks[Task.CurrentId.Value].Push(newCurrentValue);
            }
        }

        public object GetCurrent()
        {
            if (Task.CurrentId == null || !threadStacks.ContainsKey(Task.CurrentId.Value) || threadStacks[Task.CurrentId.Value].Count == 0)
                return parseStack.Peek();
            else {
                return threadStacks[Task.CurrentId.Value].Peek();
            }
        }

        public void BeginBranch()
        {
            // it's a good idea to put the parent's current to this stack
            threadStacks.TryAdd(Task.CurrentId.Value , new Stack<object>());
        }

        public object EndBranch()
        {
            Stack<object> threadStack =  null;
            threadStacks.TryRemove(Task.CurrentId.Value , out threadStack);

            if (threadStack != null && threadStack.Count > 0)
                return threadStack.Pop();
            return null;
        }

        public object Pop()
        {
            if (Task.CurrentId == null)
                return parseStack.Pop();
            else {
                var value = threadStacks[Task.CurrentId.Value].Pop();
                return value;
            }
        }

        public void FeedCurrent(string fieldName , object valueToAssign)
        {
            GetCurrent().Feed(fieldName , valueToAssign);
        }
    }
}