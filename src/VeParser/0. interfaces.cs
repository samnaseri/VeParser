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
    /// A delegate type for functions which work on an input token.
    /// </summary>
    /// <remarks>
    /// Usually the functions implementing this delegate are named like 'IsXXX' and they return true if token is XXX and return false if it is not.
    /// For example you may have a TokenParser(Of char) with the name IsDigit which says if the given char is a digit or not.
    /// </remarks>
    /// <typeparam name="TToken">It could be char or string or any other kind of token.</typeparam>
    /// <param name="token">The value to be treated.</param>
    /// <returns>A boolean value indicating whether the token parser accepted the token or not.</returns>
    public delegate bool TokenParser<TToken>(TToken token);


    public delegate void ModifyFunc(dynamic currentOutput, dynamic parserOutput);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputTarget">The parent object.</param>
    /// <param name="parserOutput">The value resulted from parsing process.</param>
    /// <returns></returns>
    public delegate dynamic UpdateFunc(dynamic currentOutput, dynamic parserOutput);

    /// <summary>
    /// Delegate for function which want to access to the current output.
    /// You may implement this deleagte to define functions that manipulate the value of current output.    
    /// </summary>
    /// <param name="currentOutput"></param>
    public delegate void ReadFunc(dynamic currentOutput);

    public delegate dynamic ReplaceFunc(dynamic currentOutput);

    public delegate dynamic Transformer(dynamic value);

}