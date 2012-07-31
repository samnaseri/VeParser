
namespace VeParser
{
    public class ParseOutput<TToken>
    {
        private int position;
        private object result;

        public ParseOutput(int position, object result)
        {
            this.position = position;
            this.result = result;
        }
        public ParseOutput(int position)
        {
            this.position = position;
            this.result = null;
        }

        public int Position
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
