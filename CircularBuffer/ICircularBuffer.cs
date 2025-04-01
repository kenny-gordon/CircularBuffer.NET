using System.Runtime.CompilerServices;

namespace CircularBuffer
{
    /// <summary>
    /// Defines the interface for a circular buffer.
    /// </summary>
    /// <typeparam name="T">The type of elements in the buffer.</typeparam>
    public interface ICircularBuffer<T> : IEnumerable<T>
    {
        #region Properties

        /// <summary>
        /// Gets the total number of elements the buffer can hold.
        /// </summary>
        int Capacity { get; }

        /// <summary>
        /// Gets the current number of elements in the buffer.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets whether the buffer is full.
        /// </summary>
        bool IsFull { get; }

        /// <summary>
        /// Gets whether the buffer is empty.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Gets the first element in the buffer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        T First
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Gets the last element in the buffer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        T Last
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
        }

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
        T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an item to the end of the buffer, overwriting the oldest item if full.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void PushBack(T item);

        /// <summary>
        /// Adds an item to the front of the buffer, overwriting the newest item if full.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void PushFront(T item);

        /// <summary>
        /// Removes and returns the item from the end of the buffer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        T PopBack();

        /// <summary>
        /// Removes and returns the item from the front of the buffer.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the buffer is empty.</exception>
        T PopFront();

        /// <summary>
        /// Adds multiple items to the end of the buffer.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The number of overwritten items.</returns>
        int PushBackRange(ReadOnlySpan<T> items);

        /// <summary>
        /// Adds multiple items to the front of the buffer.
        /// </summary>
        /// <param name="items">The items to add.</param>
        /// <returns>The number of overwritten items.</returns>
        int PushFrontRange(ReadOnlySpan<T> items);

        /// <summary>
        /// Removes multiple items from the end of the buffer.
        /// </summary>
        /// <param name="destination">The span to copy the removed items to.</param>
        /// <returns>The number of items removed.</returns>
        int PopBackRange(Span<T> destination);

        /// <summary>
        /// Removes multiple items from the front of the buffer.
        /// </summary>
        /// <param name="destination">The span to copy the removed items to.</param>
        /// <returns>The number of items removed.</returns>
        int PopFrontRange(Span<T> destination);

        /// <summary>
        /// Removes all items from the buffer.
        /// </summary>
        void Clear();

        /// <summary>
        /// Copies the buffer contents to a span.
        /// </summary>
        /// <param name="destination">The span to copy the contents to.</param>
        void CopyTo(Span<T> destination);

        /// <summary>
        /// Returns the buffer contents as an array.
        /// </summary>
        /// <returns>An array containing the buffer contents.</returns>
        T[] ToArray();

        /// <summary>
        /// Gets a span representing the buffer contents.
        /// </summary>
        /// <returns>A read-only span representing the buffer contents.</returns>
        ReadOnlySpan<T> AsSpan();

        #endregion
    }
}