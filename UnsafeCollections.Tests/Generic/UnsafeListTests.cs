using UnsafeCollections.Generic;

using System;

using Xunit;
using Xunit.Abstractions;

namespace UnsafeCollections.Tests.Generic;

public class UnsafeListTests : BaseTests
{
    private UnsafeList<int> _sut;

    public UnsafeListTests(ITestOutputHelper output) : base(output) { }

    public override void Dispose()
    {
        base.Dispose();

        GC.SuppressFinalize(this);

        _sut.Dispose();
    }

    [Fact]
    public void Constructor_InitialiseDefault_ShouldBeEmpty()
    {
        _sut = new UnsafeList<int>();

        Assert.Equal(UnsafeList<int>.Empty, _sut);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Constructor_InitialiseWithCapacity_ShouldBeEmpty(int capacity)
    {
        _sut = new UnsafeList<int>(capacity);

        if (capacity <= 0)
        {
            Assert.True(_sut.Equals(UnsafeList<int>.Empty));
        }
        else
        {
            Assert.Equal(capacity, _sut.Capacity);
        }

        Assert.Equal(0, _sut.Count);
        Assert.True(_sut.IsEmpty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void Constructor_InitialiseFromArray_ShouldBeAbleToCreateUnsafeList(int[]? array)
    {
        _sut = new UnsafeList<int>(array);

        if (array is null)
        {
            Assert.True(_sut.IsEmpty);
            Assert.Equal(0, _sut.Capacity);
            Assert.Equal(0, _sut.Count);
        }
        else
        {
            for (var i = 0; i < array.Length; i++)
            {
                Assert.True(_sut.TryGet(i, out var item));
                Assert.Equal(array[i], item);
            }

            Assert.Equal(array.Length, _sut.Capacity);
            Assert.Equal(array.Length, _sut.Count);
            Assert.True(_sut.IsFull);
        }
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Indexer_GetIndexGreaterThanCapacity_ShouldThrow(int capacity)
    {
        var additional = Random.Shared.Next(1, int.MaxValue - capacity);

        _sut = new UnsafeList<int>(capacity);

        Assert.Throws<IndexOutOfRangeException>(() => _sut[capacity + additional]);
    }
        
    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void Indexer_SetIndexWithArrayValue_ShouldMatchArray(int[] array)
    {
        _sut = new UnsafeList<int>(array.Length);

        for (var i = 0; i < array.Length; i++)
        {
            _sut[i] = array[i];
        }
            
        for (var i = 0; i < array.Length; i++)
        {
            Assert.Equal(array[i], _sut[i]);
        }
    }
        
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Indexer_SetIndexGreaterThanCapacity_ShouldThrow(int capacity)
    {
        var additional = Random.Shared.Next(1, int.MaxValue - capacity);

        _sut = new UnsafeList<int>(capacity);

        Assert.Throws<IndexOutOfRangeException>(() => _sut[capacity + additional] = Random.Shared.Next());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void TryGet_IndexGreaterThanCapacity_ShouldReturnFalse(int capacity)
    {
        var additional = Random.Shared.Next(1, int.MaxValue - capacity);

        _sut = new UnsafeList<int>(capacity);

        Assert.False(_sut.TryGet(capacity + additional, out var value));
        Assert.Equal(default, value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void TryAdd_AddItemToUnsafeList_ShouldBeAbleToExpand(int capacity)
    {
        _sut = new UnsafeList<int>(capacity);

        for (var i = 0; i < capacity; i++)
            _sut.Add(Random.Shared.Next());

        Assert.True(_sut.IsFull);
        
        _sut.Add(Random.Shared.Next());
        
        Assert.True(_sut.Capacity > capacity);
    }

    [Fact]
    public void TryRemove_RemoveItemFromUnsafeList_ShouldReturnTrue()
    {
        _sut = new UnsafeList<int>();
    
        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;
    
        _sut.Add(value1);
        _sut.Add(value2);
        Assert.True(_sut.TryRemove(value1));
        Assert.False(_sut.TryRemove(value1));
        Assert.Equal(1, _sut.Count);
    }
    
    [Fact]
    public void TryRemoveAt_RemoveItemIndex2FromUnsafeList_ShouldReturnTrue()
    {
        _sut = new UnsafeList<int>();
    
        Assert.False(_sut.TryRemoveAt(-124));
        
        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;
    
        _sut.Add(value1);
        _sut.Add(value2);
        Assert.True(_sut.TryRemoveAt(1));
        Assert.False(_sut.TryRemoveAt(1));
    
        Assert.True(_sut.TryGet(0, out var item));
        Assert.Equal(value1, item);
    }
    
    [Fact]
    public void Contains_UnsafeListCheckItems_ShouldReturnExpectedResult()
    {
        _sut = new UnsafeList<int>();
    
        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;
        var invalidValue1 = value1 - 1;
    
        _sut.Add(value1);
        _sut.Add(value2);
    
        Assert.True(_sut.Contains(value1));
        Assert.True(_sut.Contains(value2));
        Assert.False(_sut.Contains(invalidValue1));
    }
    
    [Fact]
    public void IndexOf_GetIndicesOfItemsInUnsafeList_ShouldReturnCorrectIndex()
    {
        _sut = new UnsafeList<int>();
        
        Assert.Equal(-1, _sut.IndexOf(123));
    
        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;
        var invalidValue1 = value1 - 1;
    
        _sut.Add(value1);
        _sut.Add(value2);
    
        Assert.Equal(0, _sut.IndexOf(value1));
        Assert.Equal(1, _sut.IndexOf(value2));
        Assert.Equal(-1, _sut.IndexOf(invalidValue1));
    }
    
    [Fact]
    public void Clear_UnsafeListClearItems_ShouldReturnCount0()
    {
        _sut = new UnsafeList<int>();
    
        _sut.Add(Random.Shared.Next());
        _sut.Add(Random.Shared.Next());
    
        Assert.False(_sut.IsEmpty);
    
        _sut.Clear();
    
        Assert.True(_sut.IsEmpty);
    }
    
    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder(int[] array)
    {
        _sut = new UnsafeList<int>(array);
    
        var count = 0;

        foreach (var item in _sut)
        {
            Assert.True(_sut.TryGet(count, out var value));
            Assert.Equal(value, item);
            count++;
        }
    
        Assert.Equal(count, _sut.Count);
        Assert.Equal(count, _sut.Capacity);
    }
    
    [Fact]
    public void GetPinnableReference_FixedBlockForUnsafeListWithItems_ShouldGetRefToFirstElement()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        _sut = new UnsafeList<int>(array);
    
        unsafe
        {
            fixed (int* pointer = _sut)
            {
                Assert.Equal(array[0], *pointer);
            }
        }
    }
    
    [Fact]
    public void GetPinnableReference_FixedBlockForEmptyUnsafeList_ShouldReturnZeroPointer()
    {
        _sut = new UnsafeList<int>();
    
        unsafe
        {
            fixed (int* pointer = _sut)
            {
                Assert.True(pointer == IntPtr.Zero.ToPointer());
            }
        }
    }

    [Fact]
    public void Equality_UseEqualsAndOperatorsOnEmptyUnsafeList_ShouldReturnExpectedResults()
    {
        var array = Array.Empty<int>();
    
        _sut = new UnsafeList<int>(array);
        var tempList = new UnsafeList<int>(array);
    
        Assert.True(_sut == tempList);
        Assert.True(_sut.Equals(tempList));
    
        var value = Random.Shared.Next();
        tempList.Add(value);
    
        Assert.True(_sut != tempList);
        Assert.False(_sut.Equals(tempList));
    
        Assert.False(_sut.Equals(new object()));
    }
    
    [Fact]
    public void Equality_UseEqualsAndOperatorsOnUnsafeListWithItems_ShouldReturnExpectedResults()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        
        _sut = new UnsafeList<int>(array);
        var tempList = new UnsafeList<int>(array);
    
        Assert.False(_sut == tempList);
        Assert.False(_sut.Equals(tempList));
    
        var value = Random.Shared.Next();
        tempList.Add(value);
    
        Assert.True(_sut != tempList);
        Assert.False(_sut.Equals(tempList));
    
        Assert.False(_sut.Equals(new object()));
    }
    
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void ToString_CreateUnsafeList_ShouldContainNameTypeCountAndCapacity(int capacity)
    {
        _sut = new UnsafeList<int>(capacity);
        _sut.Add(Random.Shared.Next());
    
        var toString = _sut.ToString();
    
        Assert.Contains(nameof(UnsafeList<int>), toString);
        Assert.Contains(nameof(Int32), toString);
        Assert.Contains(_sut.Count.ToString(), toString);
        Assert.Contains(_sut.Capacity.ToString(), toString);
    }
}