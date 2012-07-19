
namespace VeParser
{
    public delegate ParseOutput<TToken> ParserHandler<TToken>(ParseContext<TToken> context, ushort position);
}
