﻿using NUnit.Framework;
using Unity.Collections;
using UniNativeLinq;
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
      
    }
  }
}
