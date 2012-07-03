using System.Collections.Generic;
using System.Linq;

namespace VeParser
{
    public class Lexer : CharParser
    {
        private Parser rootParser;
        
        public Lexer(string[] keywords , string[] symbols , bool ignoreWhiteSpaces = true , TokenChecker<char> whitespaceTokenChecker = null)
        {
            var keywordParsers = from keyword in keywords
                                 select create<keyword>(@seq(
                                     from _char in keyword.ToCharArray() select fill("chars", token(_char))));
            var symbolParsers = from symbol in symbols
                                select create<symbol>(@seq(
                                    from _char in symbol.ToCharArray() select fill("chars", token(_char))));
            var numberParsers =
            new[] {
                create<number>(oneOrMore(fill("chars" , digit))) ,
                create<number>(seq(zeroOrMore(fill("chars" , digit)) , fill("chars" , token('.')) , oneOrMore(fill("chars" , digit))))
            };
            var quotation = Token('"');
            var notquotatoin = fill("chars", toParser(ch => ch != '"'));
            var qoutationEscape = seq(toParser(ch => ch == '\\'), fill("chars", token('"')));
            var textParser = seq(quotation, create<text>(zeroOrMoreAny(qoutationEscape, notquotatoin)), quotation);

            var identifierParser = create<identifier>(seq(fill("chars", letter), zeroOrMore(fill("chars", letter_or_digit))));

            Parser ignoreParsers = () => false;
            if (ignoreWhiteSpaces)
            {
                if (whitespaceTokenChecker != null)
                    Whitespace = toParser(whitespaceTokenChecker);
                ignoreParsers = oneOrMoreAny(seq(Whitespace));
            }

            var tokenParser = fill("tokens", any(
                                 @any(keywordParsers),
                                 identifierParser,
                                 @any(numberParsers),
                                 textParser,
                                 @any(symbolParsers)
                                ));

            if (ignoreWhiteSpaces)
            {
                rootParser = create<TokenList>(seq(zeroOrMoreAny(tokenParser, ignoreParsers), end_of_file()));
            }
        }

        protected override Parser GetRootParser()
        {
            return rootParser;
        }

        public TokenList Parse(string code)
        {
            return base.Parse(code.ToCharArray()) as TokenList;
        }
        public static TokenList Parser(string code, string[] keywords, string[] symbols, bool ignoreWhireSpaces = true, TokenChecker<char> whitespaceTokenChecker = null)
        {
            var lexer = new Lexer(keywords,symbols, ignoreWhireSpaces, whitespaceTokenChecker);
            return lexer.Parse(code);
        }
    }

    /// <summary>
    /// A default token implmentation.
    /// This is sample token class, you can define your own class.
    /// </summary>
    public class token
    {
        public token() { }

        public token(string tokenText)
        {
            chars = tokenText.ToCharArray().ToList();
        }

        public List<char> chars = new List<char>(); //During the parse process the parser will populate the chars field of the token, so at the end you can get the whole string by concating the chars.

        private string GetStringResultCache = null;

        public string GetString()
        {
            if (GetStringResultCache == null)
                GetStringResultCache = string.Join("" , chars.Select(c => c.ToString()).ToArray());
            return GetStringResultCache;
        }

        public override string ToString()
        {
            return this.GetString();
        }
    }

    public class TokenList
    {
        public List<token> tokens; // During the parse process the tokens array will be populated by parser
    }

    //
    // The following types are only tokens which only are different from each other by their type
    //

    public class identifier : token
    {
        public identifier(string text) : base(text) { }

        public identifier() { }
    }

    public class symbol : token
    {
        public symbol(string text) : base(text) { }

        public symbol() { }
    }

    public class number : token
    {
        public number(string text) : base(text) { }

        public number() { }
    }

    public class text : token
    {
        public text(string text) : base(text) { }

        public text() { }
    }

    // Important note to parser authors: At the lexer level the contextual keywords should not be constructed. so define only general keywords using this keyword type.

    public class keyword : token
    {
        public keyword(string text) : base(text) { }

        public keyword() { }
    }
}