using System.Collections.Generic;

namespace VeParser
{
    /// <summary>
    /// Reads tokens from an IEnumerable and caches the read tokens for further access requests.
    /// </summary>
    /// <typeparam name="TToken">The type of token in your parsing structure, it may be char or string or your own defined data structure for handling the tokens.</typeparam>
    public class EnumeratorReader<TToken>
    {
        /// <summary>
        /// Constructs an EnumeratorReader by passing a IEnumerable'1 as input.
        /// </summary>
        /// <param name="source"></param>
        public EnumeratorReader(IEnumerable<TToken> source) { this.source = source.GetEnumerator(); }

        private IEnumerator<TToken> source;
        private List<TToken> cachedList = new List<TToken>();

        /// <summary>
        /// The last readed position.
        /// </summary>
        public int LastReadedPosition { get { return cachedList.Count - 1; } }

        /// <summary>
        /// Returns the token in the specified position.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public TToken GetTokenAt(int position)
        {
            if (cachedList.Count > position)
                return cachedList[position];
            else
            {
                ReadSourceUntil(position);
                if (EndOfFileReached)
                    return default(TToken);
                return cachedList[position];
            }
        }

        /// <summary>
        /// This EndOfFileReached means that the last call to getTokenAt was for a position over the end of file. Simply put, the whole source has been read.
        /// </summary>
        public bool EndOfFileReached { get; private set; }


        /// <summary>
        /// Reads the source to the specified position and caches the read data. This method will be used by <see cref="GetTokenAt"/> to make sure that the data at the specified position has been read so far or otherwise read it.
        /// </summary>
        /// <param name="position"></param>
        private void ReadSourceUntil(int position)
        {
            lock (this)
            {
                if (EndOfFileReached)
                    return;
                var readCount = position - cachedList.Count + 1;
                for (int i = 0; i < readCount; i++)
                {
                    var nextIsAvaiblable = source.MoveNext();
                    if (nextIsAvaiblable)
                        cachedList.Add(source.Current);
                    else
                    {
                        EndOfFileReached = true;
                        return;
                    }
                }
            }
        }
    }
}