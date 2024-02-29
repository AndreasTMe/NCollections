using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using System.Collections;
using System.Collections.Generic;

using UnsafeCollections.Core;

namespace UnsafeCollections.Benchmarks.Core;

[MinColumn]
[MaxColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[OperationsPerSecond]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class UnsafeListBenchmarks
{
    [Params(0, 10, 100, 1000)]
    public int Capacity { get; set; }

#region Initialisation Benchmarks

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory("Initialisation")]
    public void Initialisation_UnsafeListWithCapacity()
    {
        using var list = new UnsafeList(Capacity, typeof(int));
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory("Initialisation")]
    public void Initialisation_Array()
    {
        var array = new int[Capacity];
    }

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory("Initialisation")]
    public void Initialisation_ArrayListWithCapacity()
    {
        var arrayList = new ArrayList(Capacity);
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory("Initialisation")]
    public void Initialisation_ListWithCapacity()
    {
        var list = new List<int>(Capacity);
    }

#endregion

#region ForEach Loop Benchmarks

    private UnsafeList _unsafeList;
    private List<int> _list = null!;
    private ArrayList _arrayList = null!;
    private int[] _array = null!;

    [GlobalSetup(
        Targets =
        [
            nameof(ForEach_UnsafeList), nameof(ForEach_Array), nameof(ForEach_ArrayList),
            nameof(ForEach_NormalList)
        ])]
    public void ForEach_Setup()
    {
        _unsafeList = new UnsafeList(Capacity, typeof(int));
        _array = new int[Capacity];
        _arrayList = new ArrayList(Capacity);
        _list = new List<int>(Capacity);

        for (var i = 0; i < Capacity; i++)
        {
            _unsafeList.TryAdd(i, false);
            _array[i] = i;
            _arrayList.Add(i);
            _list.Add(i);
        }
    }

    [GlobalCleanup(
        Targets =
        [
            nameof(ForEach_UnsafeList), nameof(ForEach_Array), nameof(ForEach_ArrayList),
            nameof(ForEach_NormalList)
        ])]
    public void ForEach_Cleanup()
    {
        _unsafeList.Dispose();
        _array = null!;
        _arrayList = null!;
        _list = null!;
    }

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_UnsafeList()
    {
        foreach (var item in _unsafeList.AsEnumerator<int>())
        {
            var temp = item;
        }
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_Array()
    {
        foreach (var item in _array)
        {
            var temp = item;
        }
    }

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_ArrayList()
    {
        foreach (var item in _arrayList)
        {
            var temp = item as int? ?? default;
        }
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_NormalList()
    {
        foreach (var item in _list)
        {
            var temp = item;
        }
    }

#endregion
}