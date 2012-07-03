using System.Collections.Generic;

namespace VeParser
{
    /// <summary>
    /// The Ve Parser is capable of parsing any data structure that implements this interface.
    /// The Ve Parser has a default implementation of this interface which will make it possible
    /// to use Ve Parser to work with any data structure which implemented IEnumerator.
    /// </summary>
    /// <typeparam name="TToken"></typeparam>
    public interface ISourceNavigator<TToken>
    {
        void ConfirmPosition();

        TToken getCurrent();

        bool IsEndOfFile { get; }

        void MoveNext();

        void RestorePosition();

        int SavePosition();

        int Position { get; }
    }

    public class SourceNavigator<TToken> : ISourceNavigator<TToken>
    {
        public SourceNavigator(EnumeratorReader<TToken> source)
        {
            this.source = source;
        }

        public EnumeratorReader<TToken> source;
        private int position;
        private Stack<int> savedPositions = new Stack<int>();

        /// <summary>
        ///
        /// </summary>
        /// <remarks>
        /// <para>
        /// <header>Managing end of file</header>
        /// After calling this method you may read the IsEndOfFile property, so if the IsEndOfFile is true then the method was not succesful and
        /// the returned value is not valuable.
        /// This method will return default of TToken if position is after the end of file. If token is a struct types use Nullable type of that struct type
        /// to get a null value whenever the end of file is passed.
        /// </para>
        /// </remarks>
        /// <returns></returns>
        public TToken getCurrent()
        {
            return source.GetTokenAt(position);
        }

        public void MoveNext()
        {
            position = position + 1;
            source.GetTokenAt(position); // This will cause source be marked as readed-to-the-end if we are in the position after the end of source
        }

        public int SavePosition()
        {
            savedPositions.Push(position);
            return position;
        }

        public void RestorePosition()
        {
            this.position = savedPositions.Pop();
        }

        public void ConfirmPosition()
        {
            savedPositions.Pop();
        }

        public bool IsEndOfFile
        {
            get
            {
                return source.EndOfFileReached  // source is read completely
                    & position > source.LastReadedPosition; // and tokens are processed to the end of file
            }
        }

        public int Position
        {
            get { return position; }
            set { position = value; }
        }
    }
}