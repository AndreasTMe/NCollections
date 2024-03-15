using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using NCollections.Core;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NCollections.Benchmarks.Core;

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

    private NativeList _nativeList;
    private List<int> _list = null!;
    private ArrayList _arrayList = null!;
    private int[] _array = null!;

    [Params(0, 10, 100, 1000, 10000)]
    public int Capacity { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _nativeList = new NativeList(Capacity, typeof(int));
        _array = new int[Capacity];
        _arrayList = new ArrayList(Capacity);
        _list = new List<int>(Capacity);

        for (var i = 0; i < Capacity; i++)
        {
            _nativeList.TryAdd(i, false);
            _array[i] = i;
            _arrayList.Add(i);
            _list.Add(i);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _nativeList.Dispose();
        _array = null!;
        _arrayList = null!;
        _list = null!;
    }

#region Initialisation Benchmarks

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(InitialisationCategory)]
    public void Initialisation_NativeListWithCapacity()
    {
        using var list = new NativeList(Capacity, typeof(int));
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

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_NativeList()
    {
        for (var i = 0; i < _nativeList.Count; i++)
        {
            _nativeList.TryGet(i, out int temp);
        }
    }

    [Benchmark(Description = "NativeList")]
    [BenchmarkCategory(ForLoopCategory)]
    public void For_NativeList_NoTypeCheck()
    {
        for (var i = 0; i < _nativeList.Count; i++)
        {
            _nativeList.TryGet(i, out int temp, false);
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

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_NativeList()
    {
        foreach (var item in _nativeList.AsEnumerator<int>())
        {
            var temp = item;
        }
    }

    [Benchmark(Description = "NativeList")]
    [BenchmarkCategory(ForEachLoopCategory)]
    public void ForEach_NativeList_NoTypeCheck()
    {
        foreach (var item in _nativeList.AsEnumerator<int>(false))
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

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_NativeList()
    {
        var contains = _nativeList.Contains<int>(Capacity / 2);
    }

    [Benchmark(Description = "NativeList")]
    [BenchmarkCategory(ContainsCategory)]
    public void Contains_NativeList_NoTypeCheck()
    {
        var contains = _nativeList.Contains<int>(Capacity / 2, false);
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

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(AddCategory)]
    public void Add_NativeList()
    {
        using var list = new NativeList(0, typeof(int));
        
        for (var i = 0; i < Capacity; i++)
        {
            list.TryAdd(i);
        }
    }
    
    [Benchmark(Description = "NativeList")]
    [BenchmarkCategory(AddCategory)]
    public void Add_NativeList_NoTypeCheck()
    {
        using var list = new NativeList(0, typeof(int));
        
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

    [Benchmark(Baseline = true, Description = "NativeList")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_NativeList()
    {
        _nativeList.TryRemove<int>(Capacity / 2);
    }

    [Benchmark(Description = "NativeList")]
    [BenchmarkCategory(RemoveCategory)]
    public void Remove_NativeList_NoTypeCheck()
    {
        _nativeList.TryRemove<int>(Capacity / 2, false);
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