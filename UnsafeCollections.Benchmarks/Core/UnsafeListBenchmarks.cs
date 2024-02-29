using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private const string InitialisationCategory = "1. Initialisation";
    private const string ForLoopCategory = "2. For Loop";
    private const string ForEachLoopCategory = "3. ForEach Loop";
    private const string ContainsCategory = "4. Contains";
    private const string AddCategory = "5. Add";
    private const string RemoveCategory = "6. Remove";

    private UnsafeList _unsafeList;
    private List<int> _list = null!;
    private ArrayList _arrayList = null!;
    private int[] _array = null!;

    [Params(0, 10, 100, 1000, 10000)]
    public int Capacity { get; set; }

    [GlobalSetup]
    public void Setup()
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

    [GlobalCleanup]
    public void Cleanup()
    {
        _unsafeList.Dispose();
        _array = null!;
        _arrayList = null!;
        _list = null!;
    }

#region Initialisation Benchmarks

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_UnsafeListWithCapacity()
    {
        using var list = new UnsafeList(Capacity, typeof(int));
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_Array()
    {
        var array = new int[Capacity];
    }

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_ArrayListWithCapacity()
    {
        var arrayList = new ArrayList(Capacity);
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_ListWithCapacity()
    {
        var list = new List<int>(Capacity);
    }

#endregion

#region Loop Benchmarks

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_UnsafeList()
    {
        for (var i = 0; i < _unsafeList.Count; i++)
        {
            _unsafeList.TryGet(i, out int temp);
        }
    }

    [Benchmark(Description = "UnsafeList")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_UnsafeList_NoTypeCheck()
    {
        for (var i = 0; i < _unsafeList.Count; i++)
        {
            _unsafeList.TryGet(i, out int temp, false);
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

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_ArrayList()
    {
        for (var i = 0; i < _arrayList.Count; i++)
        {
            var temp = _arrayList[i] as int? ?? default;
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

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_UnsafeList()
    {
        foreach (var item in _unsafeList.AsEnumerator<int>())
        {
            var temp = item;
        }
    }

    [Benchmark(Description = "UnsafeList")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_UnsafeList_NoTypeCheck()
    {
        foreach (var item in _unsafeList.AsEnumerator<int>(false))
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

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_ArrayList()
    {
        foreach (var item in _arrayList)
        {
            var temp = item as int? ?? default;
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

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_UnsafeList()
    {
        var contains = _unsafeList.Contains<int>(Capacity / 2);
    }

    [Benchmark(Description = "UnsafeList")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_UnsafeList_NoTypeCheck()
    {
        var contains = _unsafeList.Contains<int>(Capacity / 2, false);
    }

    [Benchmark(Description = "Array")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_Array()
    {
        var contains = _array.Contains(Capacity / 2);
    }

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_ArrayList()
    {
        var contains = _arrayList.Contains(Capacity / 2);
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_NormalList()
    {
        var contains = _list.Contains(Capacity / 2);
    }

#endregion
    
#region Add Benchmarks

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory(AddCategory)]
    public void Add_UnsafeList()
    {
        using var list = new UnsafeList(0, typeof(int));
        
        for (var i = 0; i < Capacity; i++)
        {
            list.TryAdd(i);
        }
    }
    
    [Benchmark(Description = "UnsafeList")]
    [BenchmarkCategory(AddCategory)]
    public void Add_UnsafeList_NoTypeCheck()
    {
        using var list = new UnsafeList(0, typeof(int));
        
        for (var i = 0; i < Capacity; i++)
        {
            list.TryAdd(i, false);
        }
    }

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory(AddCategory)]
    public void Add_ArrayList()
    {
        var arrayList = new ArrayList(0);
        
        for (var i = 0; i < Capacity; i++)
        {
            arrayList.Add(i);
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

    [Benchmark(Baseline = true, Description = "UnsafeList")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_UnsafeList()
    {
        _unsafeList.TryRemove<int>(Capacity / 2);
    }

    [Benchmark(Description = "UnsafeList")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_UnsafeList_NoTypeCheck()
    {
        _unsafeList.TryRemove<int>(Capacity / 2, false);
    }

    [Benchmark(Description = "ArrayList")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_ArrayList()
    {
        _arrayList.Remove(Capacity / 2);
    }

    [Benchmark(Description = "List")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_NormalList()
    {
        _list.Remove(Capacity / 2);
    }

#endregion
}