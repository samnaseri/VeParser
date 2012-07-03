using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VeParser
{
    interface ISourceNavigator<TToken>
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
        public SourceNavigator(SourceReader<TToken> source)
        {
            this.source = source;
        }

        public SourceReader<TToken> source;
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
            return source.getTokenAt(position);
        }

        public void MoveNext()
        {
            position = position + 1;
            source.getTokenAt(position); // This will cause source be marked as readed-to-the-end if we are in the position after the end of source
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
                    & position > source.lastReadedPosition; // and tokens are processed to the end of file
            }
        }

        public int Position
        {
            get { return position; }
            set { position = value; }
        }
    }

    public class ParallelSourceNavigator<TToken> : ISourceNavigator<TToken>
    {
        SourceReader<TToken> source;

        public ParallelSourceNavigator(SourceReader<TToken> source)
        {
            mainSourceNavigator = new SourceNavigator<TToken>(source);
            this.source = source;
        }

        SourceNavigator<TToken> mainSourceNavigator;
        ConcurrentDictionary<int,SourceNavigator<TToken>> threadSourceNavigators = new ConcurrentDictionary<int , SourceNavigator<TToken>>();

        public void ConfirmPosition()
        {
            if (Task.CurrentId == null)
                mainSourceNavigator.ConfirmPosition();
            else {
                threadSourceNavigators[Task.CurrentId.Value].ConfirmPosition();
            }
        }

        public void ConfirmBranch(int? parentThreadID)
        {
            SourceNavigator<TToken> threadSourceNavigator = null;
            threadSourceNavigators.TryRemove(Task.CurrentId.Value , out threadSourceNavigator);
            if (parentThreadID == null)
                mainSourceNavigator.Position = threadSourceNavigator.Position;
            else {
                threadSourceNavigators[parentThreadID.Value].Position = threadSourceNavigator.Position;
            }
        }

        public TToken getCurrent()
        {
            if (Task.CurrentId == null)
                return mainSourceNavigator.getCurrent();
            else {
                return threadSourceNavigators[Task.CurrentId.Value].getCurrent();
            }
        }

        public bool IsEndOfFile
        {
            get
            {
                if (Task.CurrentId == null)
                    return mainSourceNavigator.IsEndOfFile;
                else {
                    return threadSourceNavigators[Task.CurrentId.Value].IsEndOfFile;
                }
            }
        }

        public void MoveNext()
        {
            if (Task.CurrentId == null)
                mainSourceNavigator.MoveNext();
            else {
                threadSourceNavigators[Task.CurrentId.Value].MoveNext();
            }
        }

        public void RestorePosition()
        {
            if (Task.CurrentId == null)
                mainSourceNavigator.RestorePosition();
            else {
                threadSourceNavigators[Task.CurrentId.Value].RestorePosition();
            }
        }

        internal void IgnoreBranch()
        {
            SourceNavigator<TToken> threadSourceNavigator = null;
            threadSourceNavigators.TryRemove(Task.CurrentId.Value , out threadSourceNavigator);
        }

        public int SavePosition()
        {
            if (Task.CurrentId == null)
                return mainSourceNavigator.SavePosition();
            else {
                return threadSourceNavigators[Task.CurrentId.Value].SavePosition();
            }
        }

        public void StartBranch(int? parentTaskID)
        {
            int parentPosition;
            if (parentTaskID == null)
                parentPosition = mainSourceNavigator.Position;
            else
                parentPosition = threadSourceNavigators[parentTaskID.Value].Position;

            var threadSourceNavigator = new SourceNavigator<TToken>(source) { Position = parentPosition };
            threadSourceNavigators.TryAdd(Task.CurrentId.Value , threadSourceNavigator);
        }

        public int Position
        {
            get
            {
                if (Task.CurrentId == null)
                    return mainSourceNavigator.Position;
                else
                    return threadSourceNavigators[Task.CurrentId.Value].Position;
            }
        }
    }
}