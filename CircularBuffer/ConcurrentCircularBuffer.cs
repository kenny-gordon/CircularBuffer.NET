using System.Collections;
using System.Diagnostics;

namespace CircularBuffer
{
    /// <summary>
    /// Thread-safe circular buffer with multi-producer/multi-consumer support.
    /// </summary>
    /// <typeparam name="T">The type of elements in the buffer.</typeparam>
    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
    public sealed class ConcurrentCircularBuffer<T> : ICircularBuffer<T>
    {
        #region Fields

        /// <summary>
        /// The underlying circular buffer.
        /// </summary>
        private readonly CircularBuffer<T> _buffer;

        /// <summary>
        /// The reader-writer lock to ensure thread safety.
        /// </summary>
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentCircularBuffer{T}"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the circular buffer.</param>
        public ConcurrentCircularBuffer(int capacity)
        {
            _buffer = new CircularBuffer<T>(capacity);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentCircularBuffer{T}"/> class with the specified capacity and initial items.
        /// </summary>
        /// <param name="capacity">The capacity of the circular buffer.</param>
        /// <param name="items">The initial items to add to the buffer.</param>
        public ConcurrentCircularBuffer(int capacity, IEnumerable<T> items)
        {
            _buffer = new CircularBuffer<T>(capacity, items);
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public int Capacity
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer.Capacity; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <inheritdoc/>
        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer.Count; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <inheritdoc/>
        public bool IsFull
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer.IsFull; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <inheritdoc/>
        public bool IsEmpty
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer.IsEmpty; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <inheritdoc/>
        public T First
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer.First; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <inheritdoc/>
        public T Last
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer.Last; }
                finally { _lock.ExitReadLock(); }
            }
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                _lock.EnterReadLock();
                try { return _buffer[index]; }
                finally { _lock.ExitReadLock(); }
            }
            set
            {
                _lock.EnterWriteLock();
                try { _buffer[index] = value; }
                finally { _lock.ExitWriteLock(); }
            }
        }

#if DEBUG // For testing purposes
        public bool IsWriteLockHeld() => _lock.IsWriteLockHeld;
        public bool IsReadLockHeld() => _lock.IsReadLockHeld;
#endif
        #endregion

        #region Methods

        /// <inheritdoc/>
        public void PushBack(T item)
        {
            _lock.EnterWriteLock();
            try { _buffer.PushBack(item); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public void PushFront(T item)
        {
            _lock.EnterWriteLock();
            try { _buffer.PushFront(item); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public T PopBack()
        {
            _lock.EnterWriteLock();
            try { return _buffer.PopBack(); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public T PopFront()
        {
            _lock.EnterWriteLock();
            try { return _buffer.PopFront(); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public int PushBackRange(ReadOnlySpan<T> items)
        {
            _lock.EnterWriteLock();
            try { return _buffer.PushBackRange(items); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public int PushFrontRange(ReadOnlySpan<T> items)
        {
            _lock.EnterWriteLock();
            try { return _buffer.PushFrontRange(items); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public int PopBackRange(Span<T> destination)
        {
            _lock.EnterWriteLock();
            try { return _buffer.PopBackRange(destination); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public int PopFrontRange(Span<T> destination)
        {
            _lock.EnterWriteLock();
            try { return _buffer.PopFrontRange(destination); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _lock.EnterWriteLock();
            try { _buffer.Clear(); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <inheritdoc/>
        public T[] ToArray()
        {
            _lock.EnterReadLock();
            try { return _buffer.ToArray(); }
            finally { _lock.ExitReadLock(); }
        }

        /// <inheritdoc/>
        public ReadOnlySpan<T> AsSpan()
        {
            _lock.EnterReadLock();
            try { return _buffer.AsSpan(); }
            finally { _lock.ExitReadLock(); }
        }

        /// <inheritdoc/>
        public void CopyTo(Span<T> destination)
        {
            _lock.EnterReadLock();
            try { _buffer.CopyTo(destination); }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// Gets a snapshot of the buffer contents as a read-only span.
        /// </summary>
        /// <returns>A read-only span representing the buffer contents.</returns>
        public ReadOnlySpan<T> GetSnapshot()
        {
            _lock.EnterReadLock();
            try { return _buffer.ToArray().AsSpan(); }
            finally { _lock.ExitReadLock(); }
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            T[] snapshot;
            _lock.EnterReadLock();
            try { snapshot = _buffer.ToArray(); }
            finally { _lock.ExitReadLock(); }

            return ((IEnumerable<T>)snapshot).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Performs an atomic bulk operation on the underlying buffer.
        /// </summary>
        /// <param name="action">The action to perform on the buffer.</param>
        public void AtomicBulkOperation(Action<CircularBuffer<T>> action)
        {
            _lock.EnterWriteLock();
            try { action(_buffer); }
            finally { _lock.ExitWriteLock(); }
        }

        /// <summary>
        /// Performs an atomic read operation on the underlying buffer.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="func">The function to perform on the buffer.</param>
        /// <returns>The result of the function.</returns>
        public TResult AtomicReadOperation<TResult>(Func<CircularBuffer<T>, TResult> func)
        {
            _lock.EnterReadLock();
            try { return func(_buffer); }
            finally { _lock.ExitReadLock(); }
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Finalizes an instance of the <see cref="ConcurrentCircularBuffer{T}"/> class.
        /// </summary>
        ~ConcurrentCircularBuffer()
        {
            _lock.Dispose();
        }

        #endregion
    }
}