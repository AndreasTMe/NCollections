using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnsafeCollections.Core;

namespace UnsafeCollections.Benchmarks.Core;

[MinColumn]
[MaxColumn]
[OperationsPerSecond]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
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

    [IterationSetup(Target = nameof(ForEach_UnsafeList))]
    public void ForEach_UnsafeList_Setup()
    {
        _unsafeList = new UnsafeList(Capacity, typeof(int));

        for (var i = 0; i < Capacity; i++)
            _unsafeList.TryAdd(i, false);
    }

    [IterationCleanup(Target = nameof(ForEach_UnsafeList))]
    public void ForEach_UnsafeList_Cleanup() => _unsafeList.Dispose();

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_UnsafeList()
    {
        foreach (var item in _unsafeList.AsEnumerator<int>())
        {
            var temp = item;
        }
    }
    
    [IterationSetup(Target = nameof(ForEach_Array))]
    public void ForEach_Array_Setup() => _array = Enumerable.Range(0, Capacity).ToArray();

    [IterationCleanup(Target = nameof(ForEach_Array))]
    public void ForEach_Array_Cleanup() => _array = null!;

    [Benchmark(Description = "Array")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_Array()
    {
        foreach (var item in _array)
        {
            var temp = item;
        }
    }

    [IterationSetup(Target = nameof(ForEach_ArrayList))]
    public void ForEach_ArrayList_Setup()
    {
        _arrayList = new ArrayList(Capacity);
        
        for (var i = 0; i < Capacity; i++)
            _arrayList.Add(i);
    }

    [IterationCleanup(Target = nameof(ForEach_ArrayList))]
    public void ForEach_ArrayList_Cleanup() => _arrayList = null!;

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory("ForEach")]
    public void ForEach_ArrayList()
    {
        foreach (var item in _arrayList)
        {
            var temp = item as int? ?? default;
        }
    }
    
    [IterationSetup(Target = nameof(ForEach_NormalList))]
    public void ForEach_NormalList_Setup() => _list = Enumerable.Range(0, Capacity).ToList();

    [IterationCleanup(Target = nameof(ForEach_NormalList))]
    public void ForEach_NormalList_Cleanup() => _list = null!;

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