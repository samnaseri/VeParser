namespace VeParser
{
    //
    // The basic delegate types
    //

    /// <summary>
    /// The fundumental delegate which every parser function should be of this type.
    /// </summary>
    /// <returns>A boolean value indicating whether the parser was successful in parsing current token or not.</returns>
    public delegate bool Parser();

    /// <summary>
    /// A delegate type for functions which work on a input token.
    /// </summary>
    /// <remarks>
    /// Usually the functions implementing this delegate are named like 'IsXXX' and they return true if token is XXX and return false if it is not.
    /// For example you may have a TokenChecker<char> with the name IsDigit which says if the given char is a digit or not.
    /// </remarks>
    /// <typeparam name="TToken">It could be char or string or any other kind of token.</typeparam>
    /// <param name="token">The value to be treated.</param>
    /// <returns>A boolean value indicating whether the checker accepted the token or not.</returns>
    public delegate bool TokenChecker<TToken>(TToken token);
}