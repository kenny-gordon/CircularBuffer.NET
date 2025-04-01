using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using CircularBuffer.Tests.Benchmarks;

namespace CircularBuffer.Tests.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net90)]
    [MemoryDiagnoser]
    public class CoreBufferBenchmarks
    {
        private const int StandardCapacity = 1000;
        private const int TestDataSize = 100;
        private readonly int[] _testData = Enumerable.Range(0, TestDataSize).ToArray();

        [Params(typeof(CircularBuffer<int>), typeof(ConcurrentCircularBuffer<int>))]
        public Type BufferType { get; set; } = null!;

        private ICircularBuffer<int> _buffer = null!;

        [GlobalSetup]
        public void Setup()
        {
            _buffer = (ICircularBuffer<int>?)Activator.CreateInstance(BufferType, StandardCapacity)
                ?? throw new InvalidOperationException("Failed to create buffer instance.");

            // Pre-fill to 75% capacity to test both add and overwrite cases
            for (var i = 0; i < StandardCapacity * 0.75; i++)
            {
                _buffer.PushBack(i);
            }
        }

        // Single element operations
        [Benchmark]
        public void PushBackOverwrite() => _buffer.PushBack(0);

        [Benchmark]
        public void PushFrontOverwrite() => _buffer.PushFront(0);

        [Benchmark]
        public int PopBack() => _buffer.PopBack();

        [Benchmark]
        public int PopFront() => _buffer.PopFront();

        // Bulk operations
        [Benchmark]
        public int PushBackRange() => _buffer.PushBackRange(_testData);

        [Benchmark]
        public int PushFrontRange() => _buffer.PushFrontRange(_testData);

        [Benchmark]
        public int PopBackRange()
        {
            Span<int> dest = stackalloc int[TestDataSize];
            return _buffer.PopBackRange(dest);
        }

        [Benchmark]
        public int PopFrontRange()
        {
            Span<int> dest = stackalloc int[TestDataSize];
            return _buffer.PopFrontRange(dest);
        }

        // Special cases
        [Benchmark]
        public void ClearEmptyBuffer()
        {
            var tempBuffer = (ICircularBuffer<int>?)Activator.CreateInstance(BufferType, StandardCapacity)
                ?? throw new InvalidOperationException();
            tempBuffer.Clear();
        }

        [Benchmark]
        public int[] ToArrayFullBuffer() => _buffer.ToArray();
    }
}