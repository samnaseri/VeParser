
namespace VeParser
{
    public class ParseOutput<TToken>
    {
        private ushort position;
        private object result;

        public ParseOutput(ushort position, object result)
        {
            this.position = position;
            this.result = result;
        }
        public ParseOutput(ushort position)
        {
            this.position = position;
            this.result = null;
        }

        public ushort Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        public object Result { get { return result; } }

    }
}
