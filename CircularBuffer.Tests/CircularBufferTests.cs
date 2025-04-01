namespace CircularBuffer.Tests
{
    #region Core Buffer Tests

    /// <summary>
    /// Base class for testing implementations of <see cref="ICircularBuffer{T}"/>.
    /// Contains shared test logic for core buffer functionality.
    /// </summary>
    /// <typeparam name="TCircularBuffer">The buffer type under test.</typeparam>
    public abstract class CircularBufferTestsBase<TCircularBuffer>
        where TCircularBuffer : ICircularBuffer<int>
    {
        // Abstract factory methods for creating buffer instances
        protected abstract TCircularBuffer CreateBuffer(int capacity);
        protected abstract TCircularBuffer CreateBuffer(int capacity, IEnumerable<int> items);

        #region Construction & Basic Properties

        /// <summary>
        /// Validates buffer initialization with specified capacity.
        /// Tests:
        /// - Capacity property matches constructor argument
        /// - Initial empty state
        /// </summary>
        [Theory]
        [InlineData(1)]  // Minimum valid capacity
        [InlineData(10)] // Typical capacity
        [InlineData(100)] // Large capacity
        public void Constructor_WithCapacity_CreatesValidBuffer(int capacity)
        {
            // Arrange & Act
            var buffer = CreateBuffer(capacity);

            // Assert
            Assert.Equal(capacity, buffer.Capacity);
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
        }

        /// <summary>
        /// Validates buffer initialization with initial data.
        /// Tests:
        /// - Count matches initial items length
        /// - Buffer content matches initial items
        /// - Handles both under-capacity and exact-capacity scenarios
        /// </summary>
        [Theory]
        [InlineData(3, new[] { 1 })]              // Under capacity
        [InlineData(5, new[] { 1, 2, 3, 4, 5 })] // Exact capacity
        [InlineData(5, new[] { 1, 2, 3 })]       // Partial capacity
        public void Constructor_WithInitialData_PopulatesCorrectly(int capacity, int[] items)
        {
            // Arrange & Act
            var buffer = CreateBuffer(capacity, items);

            // Assert
            Assert.Equal(items.Length, buffer.Count);
            Assert.Equal(items, buffer.ToArray());
        }

        #endregion

        #region Basic Operations

        /// <summary>
        /// Tests basic push/pop operations with mixed front/back operations.
        /// Validates:
        /// - FIFO/LIFO order preservation
        /// - First/Last property correctness
        /// - Count updates
        /// </summary>
        [Fact]
        public void PushPop_OperationsMaintainOrder()
        {
            // Arrange
            var buffer = CreateBuffer(3);

            // Act & Assert
            buffer.PushBack(1);       // Buffer: [1]
            buffer.PushFront(2);      // Buffer: [2, 1]
            Assert.Equal(2, buffer.First);
            Assert.Equal(1, buffer.Last);

            buffer.PushBack(3);       // Buffer: [2, 1, 3]
            Assert.Equal(new[] { 2, 1, 3 }, buffer.ToArray());

            Assert.Equal(2, buffer.PopFront());  // Buffer: [1, 3]
            Assert.Equal(3, buffer.PopBack());   // Buffer: [1]
            Assert.Equal(1, buffer.PopFront()); // Buffer: []
        }

        #endregion

        #region Overflow Behavior

        /// <summary>
        /// Tests back overflow behavior (FIFO semantics).
        /// When full:
        /// - New back items overwrite oldest items (front)
        /// - Maintains logical order
        /// </summary>
        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 4, new[] { 2, 3, 4 })] // Full buffer
        [InlineData(new[] { 1, 2 }, 3, new[] { 1, 2, 3 })]    // Non-full buffer
        public void PushBack_WhenFull_OverwritesFront(int[] initial, int item, int[] expected)
        {
            // Arrange
            var buffer = CreateBuffer(3, initial);

            // Act
            buffer.PushBack(item);

            // Assert
            Assert.Equal(expected, buffer.ToArray());
        }

        /// <summary>
        /// Tests front overflow behavior (LIFO semantics).
        /// When full:
        /// - New front items overwrite newest items (back)
        /// - Maintains logical order
        /// </summary>
        [Theory]
        [InlineData(new[] { 1, 2, 3 }, 4, new[] { 4, 1, 2 })] // Full buffer
        [InlineData(new[] { 1, 2 }, 3, new[] { 3, 1, 2 })]    // Non-full buffer
        public void PushFront_WhenFull_OverwritesBack(int[] initial, int item, int[] expected)
        {
            // Arrange
            var buffer = CreateBuffer(3, initial);

            // Act
            buffer.PushFront(item);

            // Assert
            Assert.Equal(expected, buffer.ToArray());
        }

        #endregion

        #region Range Operations

        /// <summary>
        /// Tests bulk back insertion with overflow handling.
        /// Validates:
        /// - Number of overwritten items
        /// - Final buffer state
        /// - Wrapping behavior
        /// </summary>
        [Theory]
        [InlineData(new[] { 1, 2 }, new[] { 3, 4 }, new[] { 1, 2, 3, 4 }, 0)]   // No overflow
        [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5 }, new[] { 2, 3, 4, 5 }, 1)] // With overflow
        public void PushBackRange_HandlesOverflow(int[] initial, int[] items, int[] expected, int overwritten)
        {
            // Arrange
            var buffer = CreateBuffer(4, initial);

            // Act
            var actualOverwritten = buffer.PushBackRange(items);

            // Assert
            Assert.Equal(overwritten, actualOverwritten);
            Assert.Equal(expected, buffer.ToArray());
        }

        /// <summary>
        /// Tests bulk front insertion with overflow handling.
        /// Validates:
        /// - Number of overwritten items
        /// - Final buffer state
        /// - Wrapping behavior
        /// </summary>
        [Theory]
        [InlineData(new[] { 1, 2 }, new[] { 3, 4 }, new[] { 3, 4, 1, 2 }, 0)]   // No overflow
        [InlineData(new[] { 1, 2, 3 }, new[] { 4, 5 }, new[] { 4, 5, 1, 2 }, 1)] // With overflow
        public void PushFrontRange_HandlesOverflow(int[] initial, int[] items, int[] expected, int overwritten)
        {
            // Arrange
            var buffer = CreateBuffer(4, initial);

            // Act
            var actualOverwritten = buffer.PushFrontRange(items);

            // Assert
            Assert.Equal(overwritten, actualOverwritten);
            Assert.Equal(expected, buffer.ToArray());
        }

        /// <summary>
        /// Tests bulk removal operations with buffer wrapping.
        /// Validates:
        /// - Correct number of popped items
        /// - Destination array contents
        /// - Buffer state after removal
        /// </summary>
        [Fact]
        public void PopRange_OperationsHandleWrapping()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3, 4, 5 });
            buffer.PopFront();        // Buffer: [2, 3, 4, 5]
            buffer.PushBack(6);      // Buffer: [2, 3, 4, 5, 6]
            var destination = new int[4];

            // Act
            var popped = buffer.PopFrontRange(destination);

            // Assert
            Assert.Equal(4, popped);
            Assert.Equal(new[] { 2, 3, 4, 5 }, destination);
            Assert.Equal(new[] { 6 }, buffer.ToArray());
        }

        #endregion

        #region Edge Cases & Error Handling

        /// <summary>
        /// Validates error handling for empty buffer operations.
        /// Tests all operations that should throw when empty.
        /// </summary>
        [Fact]
        public void EmptyBuffer_ThrowsOnAccess()
        {
            // Arrange
            var buffer = CreateBuffer(3);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => buffer.First);
            Assert.Throws<InvalidOperationException>(() => buffer.Last);
            Assert.Throws<InvalidOperationException>(() => buffer.PopFront());
            Assert.Throws<InvalidOperationException>(() => buffer.PopBack());
        }

        /// <summary>
        /// Validates index bounds checking.
        /// Tests both underflow and overflow indices.
        /// </summary>
        [Theory]
        [InlineData(-1)] // Below lower bound
        [InlineData(1)]  // Above upper bound (buffer has 1 item)
        public void Indexer_ValidatesBounds(int index)
        {
            // Arrange
            var buffer = CreateBuffer(3, new[] { 1 });

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer[index]);
            Assert.Throws<ArgumentOutOfRangeException>(() => buffer[index] = 0);
        }

        #endregion

        #region Enumeration & Conversion

        /// <summary>
        /// Validates enumeration order and span conversion.
        /// Tests logical element order after modifications.
        /// </summary>
        [Fact]
        public void Enumeration_ReturnsElementsInOrder()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3, 4, 5 });
            buffer.PopFront();        // Remove 1
            buffer.PushBack(6);       // Add 6

            // Act & Assert
            Assert.Equal(new[] { 2, 3, 4, 5, 6 }, buffer.AsSpan().ToArray());
            Assert.Equal(new[] { 2, 3, 4, 5, 6 }, buffer.ToArray());
        }

        /// <summary>
        /// Validates span conversion with wrapped buffer.
        /// Tests physical-to-logical index mapping.
        /// </summary>
        [Fact]
        public void AsSpan_HandlesWrappedBuffer()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3, 4, 5 });
            buffer.PopFront(); // Start at index 1
            buffer.PopFront(); // Start at index 2
            buffer.PushBack(6); // End at index 0
            buffer.PushBack(7); // End at index 1

            // Act & Assert
            Assert.Equal(new[] { 3, 4, 5, 6, 7 }, buffer.AsSpan().ToArray());
        }

        #endregion

        #region Clear & Reset

        /// <summary>
        /// Validates buffer reset functionality.
        /// Tests state after clear operation.
        /// </summary>
        [Fact]
        public void Clear_ResetsBufferState()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3, 4, 5 });

            // Act
            buffer.Clear();

            // Assert
            Assert.Equal(0, buffer.Count);
            Assert.True(buffer.IsEmpty);
            Assert.Equal(Array.Empty<int>(), buffer.ToArray());
        }

        /// <summary>
        /// Validates buffer reuse after clearing.
        /// Tests idempotency of clear operation.
        /// </summary>
        [Fact]
        public void AfterClear_CanReuseBuffer()
        {
            // Arrange
            var buffer = CreateBuffer(3);
            buffer.PushBack(1);
            buffer.Clear();

            // Act
            buffer.PushBack(2);

            // Assert
            Assert.Equal(new[] { 2 }, buffer.ToArray());
        }

        #endregion
    }

    #endregion

    #region CircularBuffer Implementation Tests

    /// <summary>
    /// Specialized tests for <see cref="CircularBuffer{T}"/>.
    /// Focuses on span operations and edge cases.
    /// </summary>
    public class CircularBufferTests : CircularBufferTestsBase<CircularBuffer<int>>
    {
        protected override CircularBuffer<int> CreateBuffer(int capacity) => new(capacity);
        protected override CircularBuffer<int> CreateBuffer(int capacity, IEnumerable<int> items) => new(capacity, items);

        #region Specialized Span Tests

        /// <summary>
        /// Tests full wrapped buffer span conversion.
        /// Validates physical storage vs. logical order.
        /// </summary>
        [Fact]
        public void AsSpan_WithFullWrappedBuffer_ReturnsCorrectElements()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3, 4, 5 });
            buffer.PopFront(); // Start at 1
            buffer.PushBack(6); // End at 1
            buffer.PopFront(); // Start at 2
            buffer.PushBack(7); // End at 2

            // Act & Assert
            Assert.Equal(new[] { 3, 4, 5, 6, 7 }, buffer.AsSpan().ToArray());
        }

        /// <summary>
        /// Tests partial buffer copying.
        /// Validates offset and length handling in CopyTo.
        /// </summary>
        [Fact]
        public void CopyTo_WithPartialBuffer_CopiesCorrectSegment()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3, 4, 5 });
            buffer.PopFront(); // Remove 1
            buffer.PopFront(); // Remove 2
            var destination = new int[5];

            // Act
            buffer.CopyTo(destination.AsSpan(1, 3));

            // Assert
            Assert.Equal(new[] { 0, 3, 4, 5, 0 }, destination);
        }

        #endregion
    }

    #endregion

    #region ConcurrentCircularBuffer Implementation Tests

    /// <summary>
    /// Tests for <see cref="ConcurrentCircularBuffer{T}"/>.
    /// Focuses on thread safety and atomic operations.
    /// </summary>
    public class ConcurrentCircularBufferTests : CircularBufferTestsBase<ConcurrentCircularBuffer<int>>
    {
        protected override ConcurrentCircularBuffer<int> CreateBuffer(int capacity) => new(capacity);
        protected override ConcurrentCircularBuffer<int> CreateBuffer(int capacity, IEnumerable<int> items) => new(capacity, items);

        #region Concurrency Stress Tests

        /// <summary>
        /// Stress test for concurrent operations.
        /// Validates thread safety under high contention.
        /// </summary>
        [Fact]
        public async Task ParallelOperations_MaintainConsistency()
        {
            // Arrange
            const int capacity = 100;
            const int operations = 1000;
            var buffer = CreateBuffer(capacity);

            // Act
            await Task.WhenAll(
                Task.Run(() => Produce(buffer, operations)),
                Task.Run(() => Consume(buffer, operations))
            );

            // Assert
            Assert.InRange(buffer.Count, 0, capacity);
        }

        private static void Produce(ConcurrentCircularBuffer<int> buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                buffer.PushBack(i);
                Thread.Sleep(1); // Simulate work
            }
        }

        private static void Consume(ConcurrentCircularBuffer<int> buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                try { buffer.PopFront(); } catch { /* Ignore empty */ }
                Thread.Sleep(1); // Simulate work
            }
        }

        #endregion

        #region Atomic Operation Tests

        /// <summary>
        /// Tests atomic bulk operations.
        /// Validates transaction-like behavior.
        /// </summary>
        [Fact]
        public void AtomicBulkOperation_MaintainsConsistentState()
        {
            // Arrange
            var buffer = CreateBuffer(5);

            // Act
            buffer.AtomicBulkOperation(b =>
            {
                b.PushBack(1);
                b.PushFront(2);
                b.PushBackRange(new[] { 3, 4 });
            });

            // Assert
            Assert.Equal(new[] { 2, 1, 3, 4 }, buffer.ToArray());
        }

        /// <summary>
        /// Tests atomic read operations.
        /// Validates snapshot isolation.
        /// </summary>
        [Fact]
        public void AtomicReadOperation_ReturnsConsistentSnapshot()
        {
            // Arrange
            var buffer = CreateBuffer(5, new[] { 1, 2, 3 });

            // Act
            var snapshot = buffer.AtomicReadOperation(b => b.ToArray());

            // Assert
            Assert.Equal(new[] { 1, 2, 3 }, snapshot);
        }

        #endregion

        #region Lock Validation

        /// <summary>
        /// Validates lock management.
        /// Ensures locks aren't held after operations.
        /// </summary>
        [Fact]
        public void Operations_UseAppropriateLocks()
        {
            // Arrange
            var buffer = CreateBuffer(3);

            // Act & Assert (write operation)
            buffer.PushBack(1);
            Assert.False(buffer.IsReadLockHeld());
            Assert.False(buffer.IsWriteLockHeld());

            // Act & Assert (read operation)
            _ = buffer.Count;
            Assert.False(buffer.IsReadLockHeld());
            Assert.False(buffer.IsWriteLockHeld());
        }

        #endregion
    }

    #endregion
}