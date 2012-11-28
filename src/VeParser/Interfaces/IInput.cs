namespace VeParser
{
    public interface IInput<TToken>
    {
        /// <summary>
        /// When implemented should return a token at specified point.
        /// </summary>
        /// <remarks>
        /// position will start from zero and will increase one by one. However the parser may revert to an earlier position. For example when <see cref="V.Any"></see> is used then
        /// different alternative parsing grammars will be checked so, it will repeatedly start from the position in which any started, until a grammar succeed.
        /// <example>
        /// Consider implementing this interface for a string value as source stream. The the implementation will basically will use string indexer to return the char
        /// value at the desired position.
        /// </example>
        /// </remarks>
        /// <param name="position">The desired position.</param>
        /// <returns>The token at the desired position.</returns>
        TToken GetTokenAtPosition(int position);
    }
}
