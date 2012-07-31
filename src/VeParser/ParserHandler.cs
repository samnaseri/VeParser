
namespace VeParser
{
    public delegate ParseOutput<TToken> ParserHandler<TToken>(IParseContext<TToken> context, int position);
}
