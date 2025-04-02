# CircularBuffer.NET

[![Build Status](https://github.com/kenny-gordon/CircularBuffer.NET/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/kenny-gordon/CircularBuffer.NET/actions/workflows/dotnet.yml)

A circular buffer implementation for .NET.

## Overview
CircularBuffer.NET is a .NET library that provides a circular buffer data structure. A circular buffer, also known as a ring buffer, is a fixed-size buffer that works as if the memory is contiguous and has two ends as if it were connected end-to-end.

## Features
- **Fixed Size**: The buffer has a fixed size, making it memory efficient.
- **Overwrite**: Once the buffer is full, it can overwrite old data with new data.
- **Thread-Safe**: Suitable for use in multi-threaded environments.
- **Generic Implementation**: Can store any type of data.

## Usage

### Installation
To use CircularBuffer.NET in your project, add the following package reference to your project file:

```xml
<PackageReference Include="CircularBuffer.NET" Version="1.0.0" />
```

Or install it via the .NET CLI:

```sh
dotnet add package CircularBuffer.NET
```

### Basic Example
Here is a simple example of how to use the CircularBuffer.NET library:

```csharp
using CircularBuffer;

class Program
{
    static void Main()
    {
        // Create a circular buffer of integers with a capacity of 5
        var buffer = new CircularBuffer<int>(5);
        
        // Add elements to the buffer
        buffer.PushBack(1);
        buffer.PushBack(2);
        buffer.PushBack(3);
        
        // Read elements from the buffer
        foreach (var item in buffer)
        {
            Console.WriteLine(item);
        }
        
        // Output will be: 1 2 3
    }
}
```

### Advanced Usage

#### Adding Elements
You can add elements to the buffer using the `PushBack` or `PushFront` methods. If the buffer is full, the oldest element will be overwritten.

```csharp
buffer.PushBack(4);
buffer.PushBack(5);
buffer.PushBack(6); // This will overwrite the oldest element (1)
```

#### Accessing Elements
You can access elements by index using the `[]` operator.

```csharp
int firstElement = buffer[0]; // Gets the first element
int secondElement = buffer[1]; // Gets the second element
```

#### Range Operations
You can add multiple elements to the buffer using the `PushBackRange` or `PushFrontRange` methods, and remove multiple elements using the `PopBackRange` or `PopFrontRange` methods.

```csharp
var buffer = new CircularBuffer<int>(5);
buffer.PushBackRange(new[] {1, 2, 3, 4, 5});
buffer.PushFrontRange(new[] {10, 9}); // Overwrites 5 and 4

Span<int> output = stackalloc int[3];
buffer.PopFrontRange(output); // Returns 10, 9, 1
```

#### Checking the Capacity and Count
You can check the capacity and the current number of elements in the buffer.

```csharp
int capacity = buffer.Capacity; // Gets the capacity of the buffer
int count = buffer.Count; // Gets the current number of elements
```

#### Removing Elements
You can remove elements from the buffer using the `PopBack` or `PopFront` methods.

```csharp
int lastElement = buffer.PopBack(); // Removes and returns the last element
int firstElement = buffer.PopFront(); // Removes and returns the first element
```

#### Clearing the Buffer
You can clear all elements from the buffer.

```csharp
buffer.Clear();
```

#### Copying and Accessing Data
You can copy the buffer contents to a span or get a read-only span representing the buffer contents.

```csharp
Span<int> destination = stackalloc int[buffer.Count];
buffer.CopyTo(destination);

ReadOnlySpan<int> span = buffer.AsSpan();
```

#### Converting to Array
You can convert the buffer contents to an array.

```csharp
int[] array = buffer.ToArray();
```

### Thread-Safe Usage
For thread-safe operations, you can use the `ConcurrentCircularBuffer<T>`.

```csharp
var concurrentBuffer = new ConcurrentCircularBuffer<int>(5);
concurrentBuffer.PushBack(1);
concurrentBuffer.PushBack(2);
int first = concurrentBuffer.PopFront();
```

#### Atomic Bulk Operations
The `ConcurrentCircularBuffer<T>` supports atomic bulk operations.

```csharp
concurrentBuffer.AtomicBulkOperation(b => {
    b.PushBack(1);
    b.PushFront(2);
});
```

### Performance Notes
- **Non-concurrent version:** Zero allocations for core operations.
- **Concurrent version:** Adds ~20ns overhead per operation.
- **ToArray/Clear:** Allocate new arrays (use `CopyTo`/`AsSpan` for allocation-free access).

### Overwrite Behavior
Visualization of a 5-element buffer:

Initial: `[1, 2, 3, 4, 5]`

PushBack(6) => `[2, 3, 4, 5, 6]` (overwrites front)

PushFront(0) => `[0, 6, 2, 3, 4]` (overwrites back)

### Concurrency Model
The `ConcurrentCircularBuffer<T>`:
- Uses reader-writer locks for thread safety.
- Supports atomic bulk operations.
- Provides consistent snapshots via `GetSnapshot()`.

### API Reference Table
| Method           | Complexity | Notes                                    |
|------------------|------------|------------------------------------------|
| PushBack         | O(1)       | Overwrites oldest element when full      |
| PushFront        | O(1)       | Overwrites newest element when full      |
| PopBack          | O(1)       | Throws if empty                          |
| PopFront         | O(1)       | Throws if empty                          |
| PushBackRange    | O(n)       | Optimal for contiguous space             |
| PushFrontRange   | O(n)       | Optimal for contiguous space             |
| PopBackRange     | O(n)       | Removes multiple elements                |
| PopFrontRange    | O(n)       | Removes multiple elements                |
| Clear            | O(n)       | Clears all elements                      |
| ToArray          | O(n)       | Converts buffer to array                 |
| CopyTo           | O(n)       | Copies buffer contents to a span         |
| AsSpan           | O(1)       | Returns a read-only span of buffer       |

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.

## License
This project is licensed under the Apache 2.0 License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements
Special thanks to all the contributors who have helped in the development of this project.

This project was inspired by [CircularBuffer-CSharp](https://github.com/joaoportela/CircularBuffer-CSharp).
