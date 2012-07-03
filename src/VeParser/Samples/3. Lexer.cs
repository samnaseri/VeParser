using System.Collections.Generic;
using System.Linq;

namespace VeParser.Samples
{
    public class Lexer : CharParser
    {
        private Parser rootParser;

        public Lexer(string[] keywords, string[] symbols)
        {
            var keywordParsers = from keyword in keywords
                                 select createNew<Keyword>(seqParser(
                                     from _char in keyword.ToCharArray() select appendText("name", keep(token(_char)))));
            var symbolParsers = from symbol in symbols
                                select createNew<Symbol>(seqParser(
                                    from _char in symbol.ToCharArray() select appendText("name", keep(token(_char)))));
            var numberParsers =
            new[] {
                createNew<Number>(oneOrMore(appendText("name" , keep(Digit)))) ,
                createNew<Number>(seq(zeroOrMore(appendText("name" , keep(Digit))) , appendText("name", keep(token('.'))) , oneOrMore(appendText("name" , keep(Digit)))))
            };
            var quotation = token('"');
            var notquotatoin = appendText("name", not(token('"')));
            var qoutationEscape = seq(token('\\'), appendText("name", keep(token('"'))));
            var textParser = seq(quotation, createNew<Text>(zeroOrMoreAny(qoutationEscape, notquotatoin)), quotation);

            var identifierParser = createNew<Identifier>(seq(appendText("name", keep(Letter)), zeroOrMore(appendText("name", keep(Letter_or_digit)))));


            var tokenParser = add(any(
                                 anyParser(keywordParsers),
                                 identifierParser,
                                 anyParser(numberParsers),
                                 textParser,
                                 anyParser(symbolParsers)
                                ));

            rootParser = createNewList<Token>(seq(zeroOrMoreAny(tokenParser, Whitespace), endOfFile()));
        }

        protected override Parser GetRootParser()
        {
            return rootParser;
        }

        public List<Token> Parse(string code)
        {
            return base.Parse(code.ToCharArray()) as List<Token>;
        }

        public static List<Token> Parse(string code, string[] keywords, string[] symbols)
        {
            var lexer = new Lexer(keywords, symbols);
            return lexer.Parse(code);
        }
    }

    /// <summary>
    /// A default token implmentation.
    /// This is sample token class, you can define your own class.
    /// </summary>
    public class Token
    {
        public Token()
        {
            Name = "";
        }
        public Token(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    //
    // The following types are only tokens which only are different from each other by their type
    //

    public class Identifier : Token
    {
        public Identifier(string name) : base(name) { }

        public Identifier() { }
    }

    public class Symbol : Token
    {
        public Symbol(string name) : base(name) { }

        public Symbol() { }
    }

    public class Number : Token
    {
        public Number(string name) : base(name) { }

        public Number() { }
    }

    public class Text : Token
    {
        public Text(string name) : base(name) { }

        public Text() { }
    }

    // Important note to parser authors: At the lexer level the contextual keywords should not be constructed. so define only general keywords using this keyword type.

    public class Keyword : Token
    {
        public Keyword(string name) : base(name) { }

        public Keyword() { }
    }
}