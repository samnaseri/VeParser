namespace VeParser
{
    internal class Expectation<TToken>
    {
        public Expectation(params TToken[] tokens)
        {
            this.Length = tokens.Length;
            this.Tokens = tokens;
        }
        public int Length { get; set; }
        public TToken[] Tokens { get; set; }
    }
}
