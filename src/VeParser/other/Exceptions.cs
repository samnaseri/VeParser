using System;

namespace VeParser
{
    /// <summary>
    /// This exception shows that the parser execution faild because of an error in parser design.
    /// </summary>
    [Serializable]
    public class InvalidParserException : Exception
    {
        public InvalidParserException(string messag , Exception innerException)
            : base(messag , innerException)
        {
        }
    }
}