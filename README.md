# UnsafeCollections

This is a repository where I am experimenting with unsafe code in C#. I'm trying to also write benchmarks to compare the
performance of the unsafe code with the already existing code. All collections are struct wrappers around a pointer to a
memory block in native memory, so they can only store `unmanaged` types.

## UnsafeList

### Introduction

The first thing I've implemented is an `UnsafeList` struct. It is a very simple implementation, but it is already faster
than the `List<T>` class in some scenarios. It is also not generic, which makes it a lot more unsafe to use. It's more
like a "Trust me, I know what I'm doing" kind of collection.

### Why use it?

The motivation for a collection like this was experimenting with an Archetype-based Entity-Component System in C++.
The `UnsafeList` class makes way more sense there but could be used in very niche scenarios. For example, if you are
storing collections of data and you only care about their types when you are iterating/reading their data:

```csharp
public class DataStorage : IDisposable
{
    private readonly Dictionary<Type, UnsafeList> _data = new();

    public void Add<T>(T value)
        where T : unmanaged
    {
        if (!_data.TryGetValue(typeof(T), out var list))
            list = new UnsafeList(0, typeof(T));

        list.TryAdd(value);
        _data[typeof(T)] = list;
    }

    public bool TryGet<T>(out UnsafeList list)
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

Find some `UnsafeList` benchmarks [here](./.docs/unsafe-list.md).