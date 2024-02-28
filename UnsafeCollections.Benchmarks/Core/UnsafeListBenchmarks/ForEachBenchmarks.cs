using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

using System.Collections.Generic;

using UnsafeCollections.Core;

namespace UnsafeCollections.Benchmarks.Core.UnsafeListBenchmarks;

[MinColumn]
[MaxColumn]
[MemoryDiagnoser]
[NativeMemoryProfiler]
public class ForEachBenchmarks
{
    [Params(0, 10, 100, 1000)]
    public int Capacity { get; set; }
    
    private List<int> _normalList = null!;
    private UnsafeList _unsafeList;

    [GlobalSetup]
    public void Setup()
    {
        _normalList = new List<int>(Capacity);
        _unsafeList = new UnsafeList(Capacity, typeof(int));

        for (var i = 0; i < Capacity; i++)
        {
            _normalList.Add(i);
            _unsafeList.TryAdd(i, false);
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _normalList = null!;
        _unsafeList.Dispose();
    }

    [Benchmark]
    public void ForEach_NormalList()
    {
        foreach (var item in _normalList)
        {
            var temp = item;
        }
    }

    [Benchmark]
    public void ForEach_UnsafeList()
    {
        foreach (var item in _unsafeList.AsEnumerator<int>())
        {
            var temp = item;
        }
    }
}