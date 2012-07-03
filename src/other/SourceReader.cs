using System.Collections.Generic;

namespace VeParser
{
    public class SourceReader<TToken>
    {
        public SourceReader(IEnumerator<TToken> source) { this.source = source; }

        public IEnumerator<TToken> source;
        List<TToken> cachedList = new List<TToken>();

        public int lastReadedPosition { get { return cachedList.Count - 1; } }

        public TToken getTokenAt(int position)
        {
            if (cachedList.Count > position)
                return cachedList[position];
            else {
                ReadSourceUntil(position);
                if (EndOfFileReached)
                    return default(TToken);
                return cachedList[position];
            }
        }

        /// <summary>
        /// This EndOfFileReached means that the last call to getTokenAt was for a position over the end of file. Simply put, the whole source has been read.
        /// </summary>
        public bool EndOfFileReached { get; set; }

        private void ReadSourceUntil(int position)
        {
            lock (this) {
                if (EndOfFileReached)
                    return;
                var readCount = position - cachedList.Count + 1;
                for (int i = 0; i < readCount; i++) {
                    var nextIsAvaiblable = source.MoveNext();
                    if (nextIsAvaiblable)
                        cachedList.Add(source.Current);
                    else {
                        EndOfFileReached = true;
                        return;
                    }
                }
            }
        }
    }
}