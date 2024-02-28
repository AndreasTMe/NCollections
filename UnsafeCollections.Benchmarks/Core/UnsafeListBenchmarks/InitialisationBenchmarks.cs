using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;

using System.Collections.Generic;

using UnsafeCollections.Core;

namespace UnsafeCollections.Benchmarks.Core.UnsafeListBenchmarks;

[MinColumn]
[MaxColumn]
[MemoryDiagnoser]
[NativeMemoryProfiler]
public class InitialisationBenchmarks
{
    [Params(0, 10, 100, 1000)]
    public int Capacity { get; set; }

    [Benchmark]
    public void Initialisation_ListWithCapacity()
    {
        var list = new List<int>(Capacity);
    }

    [Benchmark]
    public void Initialisation_UnsafeListWithCapacity()
    {
        using var list = new UnsafeList(Capacity, typeof(int));
    }
}