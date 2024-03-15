# Native Collections

[![Tests](https://github.com/andreastdev/NCollections/actions/workflows/tests.yml/badge.svg)](https://github.com/andreastdev/NCollections/actions/workflows/tests.yml)

This is a repository where I am experimenting with unsafe code in C#. I'm trying to also write benchmarks to compare the
performance of the unsafe code with the already existing code. All collections are struct wrappers around a pointer to a
memory block in native memory, so they can only store `unmanaged` types.

#### Pros

- No GC pressure
- Equal or better performance than the standard collections (_`IndexOf` method will be improved with bitwise
  comparisons_)

#### Cons

- Need to be disposed manually
- Only for `unmanaged` types
- ...and probably more, but who cares? This is just for fun. :)

### Table of Contents

- [NativeList](#nativelist)
- [NativeList\<T>](#nativelistt)
- [NativeReadOnlyCollection\<T>](#nativereadonlycollectiont)

## NativeList

### Description

A non generic version of a `List<T>`. It looks a lot like an `ArrayList` but has generic methods to retrieve the stored
data. Generics are extremely useful, but having generics used all over the place can sometimes be a hindrance. It's more
like a "Trust me, I know what I'm doing" kind of collection.

_Find some `NativeList` benchmarks [here](./.docs/native-list.md)._

### Why use it?

The motivation for a collection like this was experimenting with an Archetype-based Entity-Component System in C++.
The `NativeList` class makes way more sense there but could be used in very niche scenarios. For example, if you are
storing collections of data and you only care about their types when you are iterating/reading their data:

```csharp
public class DataStorage : IDisposable
{
    private readonly Dictionary<Type, NativeList> _data = new();

    public void Add<T>(T value)
        where T : unmanaged
    {
        if (!_data.TryGetValue(typeof(T), out var list))
            list = new NativeList(0, typeof(T));

        list.TryAdd(value);
        _data[typeof(T)] = list;
    }

    public bool TryGet<T>(out NativeList list)
        where T : unmanaged =>
        _data.TryGetValue(typeof(T), out list);

    public void Dispose()
    {
        foreach (var list in _data.Values)
            list.Dispose();
    }
}

[Fact]
public void DataStorage_AddItemsToDataStorage_ShouldBeAbleToRetrieveItems()
{
    using var storage = new DataStorage();

    var integers = new[] { 1, 2, 3, 4, 5 };
    foreach (var value in integers)
        storage.Add(value);

    const float value3 = 1.5f;
    storage.Add(value3);

    const bool value4 = true;
    storage.Add(value4);

    Assert.True(storage.TryGet<int>(out var intList));

    var count = 0;
    foreach (var item in intList.AsEnumerator<int>())
    {
        Assert.Equal(integers[count], item);
        count++;
    }
    Assert.Equal(integers.Length, count);

    Assert.True(storage.TryGet<float>(out var floatList));
    Assert.True(floatList.TryGet(0, out float item3));
    Assert.Equal(value3, item3);

    Assert.True(storage.TryGet<bool>(out var boolList));
    Assert.True(boolList.TryGet(0, out bool item4));
    Assert.Equal(value4, item4);
}
```

---

## NativeList\<T>

### Description

Similar to a `List<T>`. It's a collection that can store a variable number of elements in a contiguous block of native
memory.

_Find some `NativeList<T>` benchmarks [here](./.docs/native-list-generic.md)._

```csharp
public struct Position
{
    public float X;
    public float Y;
    public float Z;
}

public class PositionReader
{
    public void Read(in NativeList<Position> positions)
    {
        foreach (var position in positions.AsEnumerator())
        {
            // Do something with the position
        }
    }
}
```

## NativeReadOnlyCollection\<T>

### Description

Similar to a `ReadOnlyCollection<T>`. It's a collection that can store a variable number of elements in a contiguous
block of native memory and it's read-only.

_Find some `NativeReadOnlyCollection<T>` benchmarks [here](./.docs/native-read-only-collection.md)._

```csharp
// Program.cs
var positions = new NativeList<Position>(10);

// Add some positions
// ...
// Do something with the positions
// ...

// Pass the collection to the reader
var reader = new PositionReader();
reader.Read(positions.AsReadOnly());

public struct Position
{
    public float X;
    public float Y;
    public float Z;
}

public class PositionReader
{
    public void Read(in ReadOnlyCollection<Position> positions)
    {
        foreach (var position in positions.AsEnumerator())
        {
            // Do something with the position
        }
    }
}
```