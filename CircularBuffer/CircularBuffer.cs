using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CircularBuffer
{
    /// <summary>
    /// Represents a circular buffer data structure.
    /// </summary>
    /// <typeparam name="T">The type of elements in the buffer.</typeparam>
    /// <remarks>
    /// This implementation is not thread-safe. For a thread-safe version, please use <see cref="ConcurrentCircularBuffer{T}"/>.
    /// </remarks>
    [DebuggerDisplay("Count = {Count}, Capacity = {Capacity}")]
    public sealed class CircularBuffer<T> : ICircularBuffer<T>
    {
        #region Fields

        /// <summary>
        /// The underlying array that stores the buffer elements.
        /// </summary>
        private readonly T[] _buffer;

        /// <summary>
        /// The index of the start (first) element in the buffer.
        /// </summary>
        private int _start;

        /// <summary>
        /// The index of the end (last) element in the buffer.
        /// </summary>
        private int _end;

        /// <summary>
        /// The current number of elements in the buffer.
        /// </summary>
        private int _count;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the circular buffer.</param>
        public CircularBuffer(int capacity) : this(capacity, ReadOnlySpan<T>.Empty) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class with the specified capacity and initial items.
        /// </summary>
        /// <param name="capacity">The capacity of the circular buffer.</param>
        /// <param name="items">The initial items to add to the buffer.</param>
        public CircularBuffer(int capacity, IEnumerable<T>? items) : this(capacity, (ReadOnlySpan<T>)items?.ToArray()) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CircularBuffer{T}"/> class with the specified capacity and initial items.
        /// </summary>
        /// <param name="capacity">The capacity of the circular buffer.</param>
        /// <param name="items">The initial items to add to the buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the capacity is less than 1.</exception>
        /// <exception cref="ArgumentException">Thrown when the number of initial items exceeds the capacity.</exception>
        public CircularBuffer(int capacity, ReadOnlySpan<T> items)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(capacity, 1, nameof(capacity));

            if (items.Length > capacity)
            {
                throw new ArgumentException(
                    $"Too many items ({items.Length}) for buffer capacity {capacity}",
                    nameof(items));
            }

            _buffer = new T[capacity];
            items.CopyTo(_buffer);
            _count = items.Length;
            _start = 0;
            _end = _count == capacity ? 0 : _count;
        }

        #endregion

        #region Properties

        /// <inheritdoc/>
        public int Capacity => _buffer.Length;

        /// <inheritdoc/>
        public int Count => _count;

        /// <inheritdoc/>
        public bool IsFull => _count == Capacity;

        /// <inheritdoc/>
        public bool IsEmpty => _count == 0;

        /// <inheritdoc/>
        public T First
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfEmpty();
                return _buffer[_start];
            }
        }

        /// <inheritdoc/>
        public T Last
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                ThrowIfEmpty();
                return _buffer[(_end == 0 ? Capacity : _end) - 1];
            }
        }

        /// <inheritdoc/>
        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)_count)
                {
                    ThrowIndexOutOfRange(index);
                }
                return _buffer[GetInternalIndex(index)];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if ((uint)index >= (uint)_count)
                {
                    ThrowIndexOutOfRange(index);
                }
                _buffer[GetInternalIndex(index)] = value;
            }
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushBack(T item)
        {
            if (IsFull)
            {
                OverwriteBack(item);
            }
            else
            {
                AddBack(item);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void PushFront(T item)
        {
            if (IsFull)
            {
                OverwriteFront(item);
            }
            else
            {
                AddFront(item);
            }
        }

        /// <inheritdoc/>
        public T PopBack()
        {
            ThrowIfEmpty();
            Decrement(ref _end);
            var item = _buffer[_end];
            _buffer[_end] = default!;
            _count--;
            return item;
        }

        /// <inheritdoc/>
        public bool TryPopFront(out T item)
        {
            if (IsEmpty)
            {
                item = default!;
                return false;
            }
            item = PopFront();
            return true;
        }

        /// <inheritdoc/>
        public bool TryPopBack(out T item)
        {
            if (IsEmpty)
            {
                item = default!;
                return false;
            }
            item = PopBack();
            return true;
        }

        /// <inheritdoc/>
        public bool TryPeekFront(out T item)
        {
            if (IsEmpty)
            {
                item = default!;
                return false;
            }
            item = First;
            return true;
        }

        /// <inheritdoc/>
        public bool TryPeekBack(out T item)
        {
            if (IsEmpty)
            {
                item = default!;
                return false;
            }
            item = Last;
            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T PopFront()
        {
            ThrowIfEmpty();
            var item = _buffer[_start];
            _buffer[_start] = default!;
            Increment(ref _start);
            _count--;
            return item;
        }

        /// <inheritdoc/>
        public int PushBackRange(ReadOnlySpan<T> items)
        {
            if (items.IsEmpty) return 0;

            // Determine how many items to copy. If `items` is larger than capacity, we only copy the last `Capacity` items.
            int copyLength = Math.Min(items.Length, Capacity);
            var itemsToCopy = items[^copyLength..];

            // Calculate how many existing items will be overwritten.
            int bufferOverwritten = Math.Max(0, copyLength - (Capacity - _count));

            if (bufferOverwritten > 0)
            {
                // If we're overwriting, the new start will be advanced past the overwritten items.
                _start = (_start + bufferOverwritten) % Capacity;
                _count -= bufferOverwritten;
            }

            // Copy the items into the buffer, handling wrap-around if necessary.
            if (Capacity - _end >= copyLength)
            {
                // No wrap-around, copy in a single block.
                itemsToCopy.CopyTo(_buffer.AsSpan(_end));
            }
            else
            {
                // Wrap-around, copy in two segments.
                int firstSegment = Capacity - _end;
                itemsToCopy[..firstSegment].CopyTo(_buffer.AsSpan(_end));
                itemsToCopy[firstSegment..].CopyTo(_buffer);
            }

            _end = (_end + copyLength) % Capacity;
            _count += copyLength;

            return bufferOverwritten;
        }

        /// <inheritdoc/>
        public int PushFrontRange(ReadOnlySpan<T> items)
        {
            if (items.IsEmpty) return 0;

            // Determine how many items to copy. If `items` is larger than capacity, we only copy the first `Capacity` items.
            int copyLength = Math.Min(items.Length, Capacity);
            var itemsToCopy = items[..copyLength];

            // Calculate how many existing items will be overwritten.
            int bufferOverwritten = Math.Max(0, copyLength - (Capacity - _count));

            if (bufferOverwritten > 0)
            {
                // If we're overwriting, the end will be moved back past the overwritten items.
                _end = (_end - bufferOverwritten + Capacity) % Capacity;
                _count -= bufferOverwritten;
            }

            // Move start pointer backwards to make space for new items.
            _start = (_start - copyLength + Capacity) % Capacity;

            // Copy the items into the buffer, handling wrap-around if necessary.
            if (_start + copyLength <= Capacity)
            {
                // No wrap-around, copy in a single block.
                itemsToCopy.CopyTo(_buffer.AsSpan(_start));
            }
            else
            {
                // Wrap-around, copy in two segments.
                int firstSegment = Capacity - _start;
                itemsToCopy[..firstSegment].CopyTo(_buffer.AsSpan(_start));
                itemsToCopy[firstSegment..].CopyTo(_buffer);
            }

            _count += copyLength;

            return bufferOverwritten;
        }

        /// <inheritdoc/>
        public int PopBackRange(Span<T> destination)
        {
            var countToRemove = Math.Min(destination.Length, _count);
            if (countToRemove == 0) return 0;

            // Calculate the starting index of the segment to pop from the back.
            var srcStart = (_end - countToRemove + Capacity) % Capacity;

            // Copy the items from the buffer to the destination, handling wrap-around.
            if (srcStart + countToRemove <= Capacity)
            {
                // No wrap-around, copy in a single block.
                _buffer.AsSpan(srcStart, countToRemove).CopyTo(destination);
            }
            else
            {
                // Wrap-around, copy in two segments.
                var firstSegment = Capacity - srcStart;
                _buffer.AsSpan(srcStart, firstSegment).CopyTo(destination);
                _buffer.AsSpan(0, countToRemove - firstSegment).CopyTo(destination[firstSegment..]);
            }

            // Clear the popped items from the buffer to prevent memory leaks.
            // Note: This clear operation also needs to handle wrap-around.
            if (srcStart + countToRemove <= Capacity)
            {
                _buffer.AsSpan(srcStart, countToRemove).Clear();
            }
            else
            {
                var firstSegment = Capacity - srcStart;
                _buffer.AsSpan(srcStart, firstSegment).Clear();
                _buffer.AsSpan(0, countToRemove - firstSegment).Clear();
            }

            _count -= countToRemove;
            _end = (_end - countToRemove + Capacity) % Capacity;

            return countToRemove;
        }

        /// <inheritdoc/>
        public int PopFrontRange(Span<T> destination)
        {
            var countToRemove = Math.Min(destination.Length, _count);
            if (countToRemove == 0) return 0;

            // Copy the items from the buffer to the destination, handling wrap-around.
            if (_start + countToRemove <= Capacity)
            {
                // No wrap-around, copy in a single block.
                _buffer.AsSpan(_start, countToRemove).CopyTo(destination);
            }
            else
            {
                // Wrap-around, copy in two segments.
                var firstSegment = Capacity - _start;
                _buffer.AsSpan(_start, firstSegment).CopyTo(destination);
                _buffer.AsSpan(0, countToRemove - firstSegment).CopyTo(destination[firstSegment..]);
            }

            // Clear the popped items from the buffer to prevent memory leaks.
            // Note: This clear operation also needs to handle wrap-around.
            if (_start + countToRemove <= Capacity)
            {
                _buffer.AsSpan(_start, countToRemove).Clear();
            }
            else
            {
                var firstSegment = Capacity - _start;
                _buffer.AsSpan(_start, firstSegment).Clear();
                _buffer.AsSpan(0, countToRemove - firstSegment).Clear();
            }

            _count -= countToRemove;
            _start = (_start + countToRemove) % Capacity;

            return countToRemove;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            Array.Clear(_buffer);
            _start = 0;
            _end = 0;
            _count = 0;
        }

        /// <inheritdoc/>
        public void CopyTo(Span<T> destination)
        {
            if (destination.Length < _count)
            {
                throw new ArgumentException(
                    "Destination is too short", nameof(destination));
            }

            if (_count == 0) return;

            if (_start < _end)
            {
                _buffer.AsSpan(_start, _count).CopyTo(destination);
            }
            else
            {
                var firstSegment = Capacity - _start;
                _buffer.AsSpan(_start, firstSegment).CopyTo(destination);
                _buffer.AsSpan(0, _end).CopyTo(destination[firstSegment..]);
            }
        }

        /// <inheritdoc/>
        public T[] ToArray()
        {
            if (_count == 0) return Array.Empty<T>();

            var array = new T[_count];
            CopyTo(array);
            return array;
        }

        /// <inheritdoc/>
        public ReadOnlySpan<T> FirstContiguousSpan
        {
            get
            {
                if (IsEmpty) return ReadOnlySpan<T>.Empty;
                if (_start < _end)
                {
                    return _buffer.AsSpan(_start, _count);
                }
                return _buffer.AsSpan(_start, Capacity - _start);
            }
        }

        /// <inheritdoc/>
        public ReadOnlySpan<T> SecondContiguousSpan
        {
            get
            {
                if (IsEmpty || _start < _end) return ReadOnlySpan<T>.Empty;
                return _buffer.AsSpan(0, _end);
            }
        }

        /// <inheritdoc/>
        [Obsolete("This method may cause an allocation if the buffer is wrapped. Use FirstContiguousSpan and SecondContiguousSpan instead.", error: false)]
        public ReadOnlySpan<T> AsSpan()
        {
            if (_count == 0) return ReadOnlySpan<T>.Empty;

            if (_start < _end)
            {
                return _buffer.AsSpan(_start, _count);
            }
            else
            {
                var firstSegment = _buffer.AsSpan(_start, Capacity - _start);
                var secondSegment = _buffer.AsSpan(0, _end);
                var combined = new T[_count];
                firstSegment.CopyTo(combined);
                secondSegment.CopyTo(combined.AsSpan(firstSegment.Length));
                return combined;
            }
        }


        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _count; i++)
            {
                yield return this[i];
            }
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Private Methods

        /// <summary>
        /// Adds an item to the back of the buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddBack(T item)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            _count++;
        }

        /// <summary>
        /// Adds an item to the front of the buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddFront(T item)
        {
            Decrement(ref _start);
            _buffer[_start] = item;
            _count++;
        }

        /// <summary>
        /// Overwrites the item at the back of the buffer with the new item.
        /// </summary>
        /// <param name="item">The item to overwrite with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OverwriteBack(T item)
        {
            _buffer[_end] = item;
            Increment(ref _end);
            _start = _end;
        }

        /// <summary>
        /// Overwrites the item at the front of the buffer with the new item.
        /// </summary>
        /// <param name="item">The item to overwrite with.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OverwriteFront(T item)
        {
            Decrement(ref _start);
            _end = _start;
            _buffer[_start] = item;
        }

        /// <summary>
        /// Gets the internal index in the buffer for a given external index.
        /// </summary>
        /// <param name="index">The external index.</param>
        /// <returns>The internal index.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetInternalIndex(int index)
        {
            var offset = _start + index;
            return offset < Capacity ? offset : offset - Capacity;
        }

        /// <summary>
        /// Increments the index, wrapping around if necessary.
        /// </summary>
        /// <param name="index">The index to increment.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Increment(ref int index)
        {
            if (++index == Capacity) index = 0;
        }

        /// <summary>
        /// Decrements the index, wrapping around if necessary.
        /// </summary>
        /// <param name="index">The index to decrement.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Decrement(ref int index)
        {
            index = (index == 0 ? Capacity : index) - 1;
        }

        /// <summary>
        /// Throws an exception if the buffer is empty.
        /// </summary>
        /// <param name="caller">The name of the calling member.</param>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ThrowIfEmpty([CallerMemberName] string? caller = null)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException(
                    $"Cannot perform {caller} on an empty buffer.");
            }
        }

        /// <summary>
        /// Throws an exception if the index is out of range.
        /// </summary>
        /// <param name="index">The index that is out of range.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        private void ThrowIndexOutOfRange(int index)
        {
            throw new ArgumentOutOfRangeException(nameof(index), index,
                $"Index must be between 0 and {_count - 1}.");
        }

        #endregion
    }
}