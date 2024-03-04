using NCollections.Generic;

using System;

using Xunit;
using Xunit.Abstractions;

namespace NCollections.Tests.Generic;

public class NativeReadOnlyCollectionTests : BaseTests
{
    private NativeReadOnlyCollection<int> _sut;

    public NativeReadOnlyCollectionTests(ITestOutputHelper output) : base(output) { }

    public override void Dispose()
    {
        base.Dispose();

        GC.SuppressFinalize(this);

        _sut.Dispose();
    }

    [Fact]
    public void Constructor_InitialiseDefault_ShouldBeEmpty()
    {
        _sut = new NativeReadOnlyCollection<int>();

        Assert.Equal(NativeReadOnlyCollection<int>.Empty, _sut);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Constructor_InitialiseFromEmptyNativeListWithCapacity_ShouldBeEmpty(int capacity)
    {
        _sut = new NativeList<int>(capacity).AsReadOnly();

        Assert.Equal(0, _sut.Count);
        Assert.True(_sut.IsEmpty);
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void Constructor_InitialiseFromNativeListWithArrays_ShouldBeAbleToCreateNativeReadOnlyCollection(int[]? array)
    {
        var list = new NativeList<int>(array);
        
        Assert.True(list.Capacity > 0 ? list.IsFull : list.IsEmpty);
        
        _sut = list.AsReadOnly();
        
        Assert.False(list.IsFull);
        Assert.Throws<IndexOutOfRangeException>(() => list[0]);

        if (array is null)
        {
            Assert.True(_sut.IsEmpty);
            Assert.Equal(0, _sut.Count);
        }
        else
        {
            for (var i = 0; i < array.Length; i++)
            {
                Assert.True(_sut.TryGet(i, out var item));
                Assert.Equal(array[i], item);
                
                Assert.Equal(array[i], _sut[i]);
            }

            Assert.Equal(array.Length, _sut.Count);
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Indexer_UseIndexGreaterThanCapacity_ShouldThrow(int capacity)
    {
        var additional = Random.Shared.Next(1, int.MaxValue);
    
        _sut = new NativeList<int>(capacity).AsReadOnly();
    
        Assert.Throws<IndexOutOfRangeException>(() => _sut[capacity + additional]);
    }
    
    [Fact]
    public void TryGet_GetItemFromValidAndInvalidIndices_ShouldReturnCorrectValue()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        _sut = new NativeList<int>(array).AsReadOnly();
    
        var validIndex = Random.Shared.Next(0, array.Length);
        var invalidIndex = array.Length + Random.Shared.Next(0, int.MaxValue - array.Length);
    
        Assert.True(_sut.TryGet(validIndex, out var item1));
        Assert.Equal(item1, _sut[validIndex]);
    
        Assert.False(_sut.TryGet(invalidIndex, out var item2));
        Assert.Equal(default, item2);
    }
    
    [Fact]
    public void Contains_NativeReadOnlyCollectionCheckItems_ShouldReturnExpectedResult()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        _sut = new NativeList<int>(array).AsReadOnly();
    
        Assert.True(_sut.Contains(array[0]));
        Assert.True(_sut.Contains(array[1]));
        Assert.True(_sut.Contains(array[2]));
        Assert.True(_sut.Contains(array[3]));
        Assert.True(_sut.Contains(array[4]));
        Assert.False(_sut.Contains(1234));
    }
    
    [Fact]
    public void IndexOf_GetIndicesOfItemsInNativeReadOnlyCollection_ShouldReturnCorrectIndex()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        _sut = new NativeList<int>(array).AsReadOnly();
    
        Assert.Equal(0, _sut.IndexOf(array[0]));
        Assert.Equal(1, _sut.IndexOf(array[1]));
        Assert.Equal(2, _sut.IndexOf(array[2]));
        Assert.Equal(3, _sut.IndexOf(array[3]));
        Assert.Equal(4, _sut.IndexOf(array[4]));
        Assert.Equal(-1, _sut.IndexOf(1234));
    }
    
    [Fact]
    public void IndexOf_GetIndicesOfItemsInEmptyNativeReadOnlyCollection_ShouldReturnNegativeIndex()
    {
        _sut = new NativeList<int>().AsReadOnly();
    
        Assert.Equal(-1, _sut.IndexOf(0));
    }
    
    [Fact]
    public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        _sut = new NativeList<int>(array).AsReadOnly();
    
        var count = 0;
    
        foreach (var item in _sut.AsEnumerator())
        {
            Assert.Equal(_sut[count], item);
            count++;
        }
    
        Assert.Equal(count, _sut.Count);
    }

    [Fact]
    public void Equality_UseEqualsAndOperatorsOnEmptyNativeReadOnlyCollection_ShouldReturnExpectedResults()
    {
        var array = Array.Empty<int>();
    
        _sut = new NativeList<int>(array).AsReadOnly();
        var tempList = new NativeList<int>(array).AsReadOnly();
    
        Assert.True(_sut == tempList);
        Assert.False(_sut != tempList);
        Assert.True(_sut.Equals(tempList));
    
        Assert.False(_sut.Equals(new object()));
    }
    
    [Fact]
    public void Equality_UseEqualsAndOperatorsOnNativeReadOnlyCollectionWithItems_ShouldReturnExpectedResults()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        
        _sut = new NativeList<int>(array).AsReadOnly();
        var tempList = new NativeList<int>(array).AsReadOnly();
    
        Assert.False(_sut == tempList);
        Assert.True(_sut != tempList);
        Assert.False(_sut.Equals(tempList));
    
        Assert.False(_sut.Equals(new object()));
    }
    
    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void ToString_CreateNativeReadOnlyCollection_ShouldContainNameGenericTypeCountAndCapacity(int[] array)
    {
        _sut = new NativeList<int>(array).AsReadOnly();
    
        var toString = _sut.ToString();
    
        Assert.Contains(nameof(NativeReadOnlyCollection<int>), toString);
        Assert.Contains(nameof(Int32), toString);
        Assert.Contains(_sut.Count.ToString(), toString);
    }
}