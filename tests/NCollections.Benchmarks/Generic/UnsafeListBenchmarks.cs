using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using NCollections.Generic;

using System.Collections.Generic;
using System.Linq;

namespace NCollections.Benchmarks.Generic;

[MinColumn]
[MaxColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
[OperationsPerSecond]
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
public class NativeListBenchmarks
{
    private const string InitialisationCategory = "1. Initialisation";
    private const string ForLoopCategory = "2. For Loop";
    private const string ForEachLoopCategory = "3. ForEach Loop";
    private const string ContainsCategory = "4. Contains";
    private const string AddCategory = "5. Add";
    private const string RemoveCategory = "6. Remove";

    private NativeList<int> _unsafeList;
    private int[] _array = null!;
    private List<int> _list = null!;

    [Params(0, 10, 100, 1000, 10000)]
    public int Capacity { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _unsafeList = new NativeList<int>(Capacity);
        _array = new int[Capacity];
        _list = new List<int>(Capacity);

        for (var i = 0; i < Capacity; i++)
        {
            _unsafeList.Add(i);
            _array[i] = i;
            _list.Add(i);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _unsafeList.Dispose();
        _array = null!;
        _list = null!;
    }

#region Initialisation Benchmarks

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_NativeListWithCapacity()
    {
        using var list = new NativeList<int>(Capacity);
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_Array()
    {
        var array = new int[Capacity];
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_ListWithCapacity()
    {
        var list = new List<int>(Capacity);
    }

#endregion

#region Loop Benchmarks

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_NativeList()
    {
        for (var i = 0; i < _unsafeList.Count; i++)
        {
            _unsafeList.TryGet(i, out int temp);
        }
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_Array()
    {
        for (var i = 0; i < _array.Length; i++)
        {
            var temp = _array[i];
        }
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_NormalList()
    {
        for (var i = 0; i < _list.Count; i++)
        {
            var temp = _list[i];
        }
    }

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_NativeList()
    {
        foreach (var item in _unsafeList.AsEnumerator())
        {
            var temp = item;
        }
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_Array()
    {
        foreach (var item in _array)
        {
            var temp = item;
        }
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_NormalList()
    {
        foreach (var item in _list)
        {
            var temp = item;
        }
    }

#endregion

#region Contains Benchmarks

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_NativeList()
    {
        var contains = _unsafeList.Contains(Capacity / 2);
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_Array()
    {
        var contains = _array.Contains(Capacity / 2);
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_NormalList()
    {
        var contains = _list.Contains(Capacity / 2);
    }

#endregion
    
#region Add Benchmarks

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(AddCategory)]
    public void Add_NativeList()
    {
        using var list = new NativeList<int>(0);
        
        for (var i = 0; i < Capacity; i++)
        {
            list.Add(i);
        }
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(AddCategory)]
    public void Add_List()
    {
        var list = new List<int>(0);
        
        for (var i = 0; i < Capacity; i++)
        {
            list.Add(i);
        }
    }

#endregion
    
#region Remove Benchmarks

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_NativeList()
    {
        _unsafeList.TryRemove(Capacity / 2);
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_NormalList()
    {
        _list.Remove(Capacity / 2);
    }

#endregion
}