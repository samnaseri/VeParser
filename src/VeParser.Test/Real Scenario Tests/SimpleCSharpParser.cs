using System;

namespace VeParser.Test
{
    public class SimpleCSharpParser
    {
        public static void Parse(string code)
        {

            // Parse into code characters
            //      Each code character
            //          1. Charcter   2.Line   3. Column

            // Tokenize
            // Tokenization should consider C# keywords

            var csharp_keywords = new[] { "using", "namespace", "class", "private", "public", "protected", "internal", "event", "delegate", "var", "static" };
            Parser<CodeCharacter> tokenizer;


            Parser<Token> usingStatement, nameSpaceBlock, classDefinition, interfaceDefinition, methodSingnatureDefinition,
                methodBodyDefinition, fullMethodDefinition, fieldDefinition, propertyDefinition, autoImplemnetedPropertyDefinition,
                eventDefinition, delegateDefinition, quilifiedName;
            const int word = 1;

            Func<string, Parser<Token>> keyword = expectedKeyword => V.If<Token>(token => token.Type == word && token.Content == expectedKeyword);
            Parser<Token> identifier = V.If<Token>(token => token.Type == (int)word);
            quilifiedName = null;
            usingStatement = keyword("using") + quilifiedName;

        }
    }
}
