using System;
using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    public class TokenCheckerData { public string ExpectedToken; }

    public class ParserModel
    {
        public List<object> first = new List<object>();

        public string ActionDescription;

        public string Definition;

        public IEnumerable<Parser> SubParsers { get; set; }

        public IEnumerable<ParserModel> SubParserModels
        {
            get
            {
                if (SubParsers == null)
                    return null;
                return SubParsers.Select(p => p.GetModel()).ToArray();
            }
        }

        public string GetFullDefinition()
        {
            var result = "";
            if (SubParserModels == null)
                result = Definition;
            else {
                if (Definition.Contains("{0}"))
                    result = string.Format(Definition , SubParserModels.Select(m => m.GetFullDefinition()).ToArray());
                else {
                    result = Definition + string.Join("" , SubParserModels.Select(m => "\n" + m.GetFullDefinition().IndentText())) + "\n";
                }
            }
            if (string.IsNullOrEmpty(result))
                result = "Definition not avialable or User Defined Parser";
            return result;
        }
    }

    public static class ParserExtensions
    {
        static Dictionary<WeakReference,ParserModel> models = new Dictionary<WeakReference , ParserModel>();

        public static void SetModel(this Parser parser , string definition , Parser subParser)
        {
            // clean the models from old parsers
            var model = parser.GetModel();
            model.Definition = definition;
            model.SubParsers = new[] { subParser };
        }

        public static void SetModel(this Parser parser , string definition , IEnumerable<Parser> subParsers = null)
        {
            // clean the models from old parsers
            var model = parser.GetModel();
            model.Definition = definition;
            model.SubParsers = subParsers != null ? subParsers.ToArray() : null;
        }

        public static ParserModel GetModel(this Parser parser)
        {
            // clean the models from old parsers
            models.Select(entry => entry.Key).Where(key => !key.IsAlive).ToList().ForEach(key => models.Remove(key));
            ParserModel model = null;
            model = models.Where(entry => entry.Key.Target == parser).Select(entry => entry.Value).SingleOrDefault();
            if (model == null) {
                model = new ParserModel();
                models.Add(new WeakReference(parser) , model);
            }
            return model;
        }

        public static void SetActionDescription(this Parser parser , string value)
        {
            GetModel(parser).ActionDescription = value;
        }

        public static string GetActionDescription(this Parser parser)
        {
            var result = GetModel(parser).ActionDescription;
            if (string.IsNullOrEmpty(result))
                return "{Not-Described Rule}";
            else
                return result;
        }
    }

    public static class TokenCheckerExtensions
    {
        static Dictionary<object,TokenCheckerData> models = new Dictionary<object , TokenCheckerData>();

        public static void SetExpectedToken(this object tokenChecker , string value)
        {
            if (!models.ContainsKey(tokenChecker))
                models.Add(key: tokenChecker , value: new TokenCheckerData());
            models[tokenChecker].ExpectedToken = value;
        }

        public static string GetExpectedToken(this object tokenChecker)
        {
            if (!models.ContainsKey(tokenChecker))
                return "{Unknown Token}";
            else
                return models[tokenChecker].ExpectedToken;
        }
    }
}