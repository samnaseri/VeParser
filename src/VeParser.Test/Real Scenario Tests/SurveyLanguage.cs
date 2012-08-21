using System;
using System.Collections.Generic;
using NUnit.Framework;
using VeParser.Predefined;

namespace VeParser.Test
{

    public class Token
    {
        public int Line { get; set; }
        public int StartColumn { get; set; }
        public int FinishColumn { get; set; }
        public string Content { get; set; }
        public int Type { get; set; }
    }

    [TestFixture]
    public class Survey_Tests : VeParserTests
    {
        [Test]
        public void GetFileCharacters()
        {
            var input = SampleCode1;

            List<CodeCharacter> characters = null;

            characters = (List<CodeCharacter>)SpecialParsers.GetCodeCharactersParser().Run(SampleCode1);
            Assert.NotNull(characters);

            var lines = (List<IndentedLine>)SpecialParsers.GetIndentedLinesParser().Run(characters.ToArray());
        }

        public Parser<Token> GetSurveyParser()
        {
            Parser<Token> File, InitialStatement, MiddleStatement, Include, Enum, Question, Function, Rule, Branch, AlternativeBranches, On, Indent, Identifier;
            Func<string, Func<Token, bool>> Word = word => ((Token token) => token.Type == 1 && token.Content == word);
            Identifier = (Parser<Token>)((Token token) => token.Type == 1);

            Include = Word("Include") + Identifier;
            Enum = "Enum" + Word;
            Question = "Question";
            Function = "Function";
            Rule = "Rule";
            Branch = "Branch";
            AlternativeBranches = "Alternative" + "Branches";
            On = "On" + Identifier + ((Parser<Token>)"Changed" | "Loaded" | "Unloaded");
            InitialStatement = Include;
            MiddleStatement = Enum | Question | Function | Rule | Branch | AlternativeBranches | On;

            File = InitialStatement.Star() + MiddleStatement.Star();

            return File;
        }

        [Test]
        public void Test1()
        {
            //var output = runParser(GetSurveyParser(), SampleCode1);
            //Assert.NotNull(output);
        }


        string SampleCode1 = @"
Include General Actions
Include General Rules

Enum Sponsor Types
    Relative
    State

Question
    Name : Terms And Conditions
    Type : yes/no
    Text : Do you accept terms and conditions?
    Options : Yes, No
    Validation : Answer = Yes 

Question
    Name : Birth of Date
    Type : Single Value Question
    Data Type : Datetime
    Text : Please enter your birth date

Question 
    Name : Sponsor Type
    Type : Combo
    Text : Please specify your sponsor:
    Options : Enum(Sponsor Types)

Question
    Name : Relative Sponsor
    Type : Combo
    Text : Please specify your sponsor:
    Options : Brother, Sister, Father, Mother
    Branch : Relative Sponsorship

Function Age(DateTime birthDate)
    birthDate - DateTime.Now

Rule Age
    Age(Date of Birth Answer)

Rule Total Points
    Sum(Age Points, Language Points, IELTS Points, Employment Points)

Action Go Next Page
    PageNumber ++

Action Go Previous Page
    PageNumber --

Branch State Sponsorship

Branch Relative Sponsorship

Alternative Branches 
    Branch State Sponsorship
    Branch Relative Sponsorship

On Sponsor Type Changed
    Switch Sponsor Type 
        Case Relative   : Activate Branch Relative Sponsorship
        Case State      : Activate Branch State Sponsorship

";
    }
}
