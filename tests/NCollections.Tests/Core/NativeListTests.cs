using NCollections.Core;

using System;

using Xunit;
using Xunit.Abstractions;

namespace NCollections.Tests.Core;

public class NativeListTests : BaseTests
{
    private NativeList _sut;

    public NativeListTests(ITestOutputHelper output) : base(output) { }

    public override void Dispose()
    {
        base.Dispose();

        GC.SuppressFinalize(this);

        _sut.Dispose();
    }

    [Fact]
    public void Constructor_InitialiseDefault_ShouldBeVoid()
    {
        _sut = new NativeList();

        Assert.Equal(NativeList.Void, _sut);
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
        _sut = new NativeList(capacity, typeof(int));

        if (capacity <= 0)
        {
            Assert.False(_sut.Equals(NativeList.Void));
        }
        else
        {
            Assert.Equal(capacity, _sut.Capacity);
        }

        Assert.Equal(typeof(int).TypeHandle, _sut.TypeHandle);
        Assert.Equal(0, _sut.Count);
        Assert.True(_sut.IsEmpty);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void Constructor_InitialiseFromArray_ShouldBeAbleToCreateNativeList(int[]? array)
    {
        _sut = NativeList.From<int>(array);

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
                Assert.False(_sut.TryGet(i, out float _));
                Assert.True(_sut.TryGet(i, out int item));

                Assert.Equal(array[i], item);
            }

            Assert.Equal(array.Length, _sut.Capacity);
            Assert.Equal(array.Length, _sut.Count);
            Assert.True(_sut.Capacity > 0 ? _sut.IsFull : _sut.IsEmpty);
        }
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

        _sut = new NativeList(capacity, typeof(int));

        Assert.False(_sut.TryGet<float>(capacity + additional, out var invalid));
        Assert.Equal(default, invalid);
        Assert.False(_sut.TryGet<int>(capacity + additional, out var value));
        Assert.Equal(default, value);
    }

    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void TryGet_InitialiseFromArray_ShouldMatchArray(int[] array)
    {
        _sut = NativeList.From<int>(array);

        for (var i = 0; i < array.Length; i++)
        {
            Assert.True(_sut.TryGet(i, out int item));
            Assert.Equal(array[i], item);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void TryAdd_AddItemToNativeList_ShouldReturnTrue(int capacity)
    {
        _sut = new NativeList(capacity, typeof(int));

        for (var i = 0; i < capacity; i++)
            Assert.True(_sut.TryAdd(Random.Shared.Next()));

        Assert.True(_sut.Capacity > 0 ? _sut.IsFull : _sut.IsEmpty);
        Assert.True(_sut.TryAdd(Random.Shared.Next()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void TryAdd_AddInvalidItemToNativeList_ShouldReturnFalse(int capacity)
    {
        _sut = new NativeList(capacity, typeof(int));

        Assert.False(_sut.TryAdd(1.0f));
    }

    [Fact]
    public void TryRemove_RemoveItemFromNativeList_ShouldReturnTrue()
    {
        _sut = new NativeList(0, typeof(int));

        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;

        Assert.True(_sut.TryAdd(value1));
        Assert.True(_sut.TryAdd(value2));
        Assert.True(_sut.TryRemove(value1));
        Assert.False(_sut.TryRemove(value1));
        Assert.False(_sut.TryRemove(1.0f));
    }

    [Fact]
    public void TryRemoveAt_RemoveItemIndex2FromNativeList_ShouldReturnTrue()
    {
        _sut = new NativeList(0, typeof(int));

        Assert.False(_sut.TryRemoveAt<int>(-124));
        
        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;

        Assert.True(_sut.TryAdd(value1));
        Assert.True(_sut.TryAdd(value2));
        Assert.True(_sut.TryRemoveAt<int>(1));
        Assert.False(_sut.TryRemoveAt<int>(1));

        Assert.True(_sut.TryGet(0, out int item));
        Assert.Equal(value1, item);
    }

    [Fact]
    public void Contains_NativeListCheckItems_ShouldReturnExpectedResult()
    {
        _sut = new NativeList(0, typeof(int));

        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;
        var invalidValue1 = value1 - 1;
        var invalidValue2 = 1.0f;

        Assert.True(_sut.TryAdd(value1));
        Assert.True(_sut.TryAdd(value2));

        Assert.True(_sut.Contains(value1));
        Assert.True(_sut.Contains(value2));
        Assert.False(_sut.Contains(invalidValue1));
        Assert.False(_sut.Contains(invalidValue2));
    }

    [Fact]
    public void IndexOf_GetIndicesOfItemsInNativeList_ShouldReturnCorrectIndex()
    {
        _sut = new NativeList(0, typeof(int));

        var value1 = Random.Shared.Next();
        var value2 = value1 + 1;

        Assert.True(_sut.TryAdd(value1));
        Assert.True(_sut.TryAdd(value2));
        var invalidValue1 = value1 - 1;
        var invalidValue2 = 1.0f;

        Assert.Equal(0, _sut.IndexOf(value1));
        Assert.Equal(1, _sut.IndexOf(value2));
        Assert.Equal(-1, _sut.IndexOf(invalidValue1));
        Assert.Equal(-1, _sut.IndexOf(invalidValue2));
    }

    [Fact]
    public void Clear_NativeListClearItems_ShouldReturnCount0()
    {
        _sut = new NativeList(0, typeof(int));

        Assert.True(_sut.TryAdd(Random.Shared.Next()));
        Assert.True(_sut.TryAdd(Random.Shared.Next()));

        Assert.False(_sut.IsEmpty);

        _sut.Clear();

        Assert.True(_sut.IsEmpty);
    }

    [Theory]
    [InlineData(new int[0])]
    [InlineData(new[] { 1, 2, 3, 4, 5 })]
    public void GetEnumerator_ForeachLoop_ShouldGoThroughAllElementsInOrder(int[] array)
    {
        _sut = NativeList.From<int>(array);

        var count = 0;

        Assert.Throws<InvalidOperationException>(() => _sut.AsEnumerator<float>());

        foreach (var item in _sut.AsEnumerator<int>())
        {
            Assert.True(_sut.TryGet(count, out int value));
            Assert.Equal(value, item);
            count++;
        }

        Assert.Equal(count, _sut.Count);
        Assert.Equal(count, _sut.Capacity);
    }

    [Fact]
    public void GetPinnableReference_FixedBlockForNativeListWithItems_ShouldGetRefToFirstElement()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        _sut = NativeList.From<int>(array);

        unsafe
        {
            Assert.Throws<InvalidOperationException>(() => _sut.AsFixed<float>());

            fixed (int* pointer = _sut.AsFixed<int>())
            {
                Assert.Equal(array[0], *pointer);
            }
        }
    }

    [Fact]
    public void GetPinnableReference_FixedBlockForEmptyNativeList_ShouldReturnZeroPointer()
    {
        _sut = new NativeList(0, typeof(int));

        unsafe
        {
            fixed (int* pointer = _sut.AsFixed<int>())
            {
                Assert.True(pointer == IntPtr.Zero.ToPointer());
            }
        }
    }

    [Fact]
    public void GetPinnableReference_FixedBlockForVoidNativeList_ShouldThrow()
    {
        _sut = NativeList.Void;

        Assert.Throws<InvalidOperationException>(() => _sut.AsFixed<int>());
    }

    [Fact]
    public void Equality_UseEqualsAndOperatorsOnEmptyNativeList_ShouldReturnExpectedResults()
    {
        var array = Array.Empty<int>();

        _sut = NativeList.From<int>(array);
        var tempList = NativeList.From<int>(array);

        Assert.True(_sut == tempList);
        Assert.True(_sut.Equals(tempList));

        var value = Random.Shared.Next();
        Assert.True(tempList.TryAdd(value));

        Assert.True(_sut != tempList);
        Assert.False(_sut.Equals(tempList));

        Assert.False(_sut.Equals(new object()));
    }

    [Fact]
    public void Equality_UseEqualsAndOperatorsOnNativeListWithItems_ShouldReturnExpectedResults()
    {
        var array = new[] { 1, 2, 3, 4, 5 };
        
        _sut = NativeList.From<int>(array);
        var tempList = NativeList.From<int>(array);

        Assert.False(_sut == tempList);
        Assert.False(_sut.Equals(tempList));

        var value = Random.Shared.Next();
        Assert.True(tempList.TryAdd(value));

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
    public void ToString_CreateNativeList_ShouldContainNameTypeCountAndCapacity(int capacity)
    {
        _sut = new NativeList(capacity, typeof(int));
        _sut.TryAdd(Random.Shared.Next());

        var toString = _sut.ToString();

        Assert.Contains(nameof(NativeList), toString);
        Assert.Contains(nameof(Int32), toString);
        Assert.Contains(_sut.Count.ToString(), toString);
        Assert.Contains(_sut.Capacity.ToString(), toString);
    }
}