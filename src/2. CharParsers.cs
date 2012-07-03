namespace VeParser
{
    public abstract class CharParser : BaseParser<char>
    {
        public CharParser()
        {
            letter.SetExpectedToken("{a letter}");
            digit.SetExpectedToken("{a digit}");
            letter_or_digit.SetExpectedToken("{a letter or a digit}");
            whitespace.SetExpectedToken("{a whitespace}");
            anyChar.SetExpectedToken("{any character}");

            Letter_or_digit = toParser(letter_or_digit);
            Letter = toParser(letter);
            Digit = toParser(digit);
            Whitespace = toParser(whitespace);
            AnyChar = toParser(anyChar);
            NewLine = toParser(newLine);
        }

        public static TokenChecker<char> letter = next => char.IsLetter(next);
        public static  TokenChecker<char> digit = next => char.IsDigit(next);
        public static  TokenChecker<char> letter_or_digit = next => char.IsLetterOrDigit(next);
        public static  TokenChecker<char> whitespace = next => char.IsWhiteSpace(next);
        public static  TokenChecker<char> anyChar = next => true;
        public static  TokenChecker<char> newLine = next => next == '\n';

        protected Parser Letter_or_digit;
        protected Parser Letter;
        protected Parser Digit;
        protected Parser Whitespace;
        protected Parser AnyChar;
        protected Parser NewLine;
    }
}