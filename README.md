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
        buffer.Add(1);
        buffer.Add(2);
        buffer.Add(3);
        
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
You can add elements to the buffer using the `Add` method. If the buffer is full, the oldest element will be overwritten.

```csharp
buffer.Add(4);
buffer.Add(5);
buffer.Add(6); // This will overwrite the oldest element (1)
```

#### Accessing Elements
You can access elements by index using the `[]` operator or the `ElementAt` method.

```csharp
int firstElement = buffer[0]; // Gets the first element
int secondElement = buffer.ElementAt(1); // Gets the second element
```

#### Checking the Capacity and Count
You can check the capacity and the current number of elements in the buffer.

```csharp
int capacity = buffer.Capacity; // Gets the capacity of the buffer
int count = buffer.Count; // Gets the current number of elements
```

#### Clearing the Buffer
You can clear all elements from the buffer.

```csharp
buffer.Clear();
```

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.

## License
This project is licensed under the Apache 2.0 License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements
Special thanks to all the contributors who have helped in the development of this project.

This project was inspired by [CircularBuffer-CSharp](https://github.com/joaoportela/CircularBuffer-CSharp).
