using NUnit.Framework;
using UniNativeLinq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Tests
{
  public sealed unsafe class NativeEnumerableTestScript
  {
    [Test]
    public void DefaultValuePass()
    {
      NativeEnumerable<int> nativeEnumerable = default;
      Assert.AreEqual(0L, nativeEnumerable.Length);
      Assert.IsTrue(nativeEnumerable.Ptr == null);
    }

    [TestCase(0L)]
    [TestCase(-10L)]
    [TestCase(-12241L)]
    [TestCase(long.MinValue)]
    public void ZeroOrNegativeCountTest(long count)
    {
      using (var array = new NativeArray<int>(1, Allocator.Persistent))
      {
        Assert.IsFalse(array.GetUnsafePtr() == null);
        var nativeEnumerable = new NativeEnumerable<int>((int*) array.GetUnsafePtr(), count);
        Assert.AreEqual(0L, nativeEnumerable.Length);
        Assert.IsTrue(nativeEnumerable.Ptr == null);  
      }
    }

    [TestCase(0, Allocator.Temp)]
    [TestCase(1, Allocator.Temp)]
    [TestCase(10, Allocator.Temp)]
    [TestCase(114514, Allocator.Temp)]
    [TestCase(0, Allocator.TempJob)]
    [TestCase(1, Allocator.TempJob)]
    [TestCase(10, Allocator.TempJob)]
    [TestCase(114514, Allocator.TempJob)]
    [TestCase(0, Allocator.Persistent)]
    [TestCase(1, Allocator.Persistent)]
    [TestCase(10, Allocator.Persistent)]
    [TestCase(114514, Allocator.Persistent)]
    public void FromNativeArrayPass(int count, Allocator allocator)
    {
      using (var array = new NativeArray<int>(count, allocator))
      {
        var nativeEnumerable = new NativeEnumerable<int>((int*) array.GetUnsafePtr(), array.Length);
        Assert.AreEqual((long) count, nativeEnumerable.Length);
        for (var i = 0; i < nativeEnumerable.Length; i++)
        {
          Assert.AreEqual(0, nativeEnumerable[i]);
          nativeEnumerable[i] = i;
        }
        for (var i = 0; i < count; i++)
          Assert.AreEqual(i, array[i]);
      }
    }

    [TestCase(0, Allocator.Temp)]
    [TestCase(114, Allocator.Temp)]
    [TestCase(114514, Allocator.Temp)]
    [TestCase(0, Allocator.TempJob)]
    [TestCase(114, Allocator.TempJob)]
    [TestCase(114514, Allocator.TempJob)]
    [TestCase(0, Allocator.Persistent)]
    [TestCase(114, Allocator.Persistent)]
    [TestCase(114514, Allocator.Persistent)]
    public void IEnumerableTest(int count, Allocator allocator)
    {
      using (var array = new NativeArray<long>(count, allocator))
      {
        var nativeEnumerable = new NativeEnumerable<long>((long*) array.GetUnsafePtr(), array.Length);
        Assert.AreEqual(count, nativeEnumerable.Length);
        for (var i = 0L; i < count; i++)
        {
          nativeEnumerable[i] = i;
        }
        var index = 0L;
        foreach (ref var i in nativeEnumerable)
        {
          Assert.AreEqual(index++, i);
          i = index;
        }
        index = 1L;
        foreach (var i in nativeEnumerable)
        {
          Assert.AreEqual(index++, i);
        }
      }
    }
  }
}