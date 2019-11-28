using NUnit.Framework;

namespace Tests
{
    public sealed unsafe class NativeEnumerableTestScript
  {
    [Test]
    public void DefaultValuePass()
    {
      
    }

    // [TestCase(0L)]
    // [TestCase(-10L)]
    // [TestCase(-12241L)]
    // [TestCase(long.MinValue)]
    [Test]
    public void ZeroOrNegativeCountTest()
    {
      
    }

    // [TestCase(0, Allocator.Temp)]
    // [TestCase(1, Allocator.Temp)]
    // [TestCase(10, Allocator.Temp)]
    // [TestCase(114514, Allocator.Temp)]
    // [TestCase(0, Allocator.TempJob)]
    // [TestCase(1, Allocator.TempJob)]
    // [TestCase(10, Allocator.TempJob)]
    // [TestCase(114514, Allocator.TempJob)]
    // [TestCase(0, Allocator.Persistent)]
    // [TestCase(1, Allocator.Persistent)]
    // [TestCase(10, Allocator.Persistent)]
    // [TestCase(114514, Allocator.Persistent)]
    [Test]
    public void FromNativeArrayPass()
    {
      
    }

    // [TestCase(0, Allocator.Temp)]
    // [TestCase(114, Allocator.Temp)]
    // [TestCase(114514, Allocator.Temp)]
    // [TestCase(0, Allocator.TempJob)]
    // [TestCase(114, Allocator.TempJob)]
    // [TestCase(114514, Allocator.TempJob)]
    // [TestCase(0, Allocator.Persistent)]
    // [TestCase(114, Allocator.Persistent)]
    // [TestCase(114514, Allocator.Persistent)]
    [Test]
    public void IEnumerableTest()
    {
      
    }
  }
}