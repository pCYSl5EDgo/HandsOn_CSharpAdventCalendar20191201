using System.Collections;
using System.Collections.Generic;

namespace UniNativeLinq
{
  public readonly unsafe partial struct NativeEnumerable<T>
    : IRefEnumerable<NativeEnumerable<T>.Enumerator, T>
    where T : unmanaged
  {
    public readonly T* Ptr;
    public readonly long Length;

    public NativeEnumerable(T* ptr, long length)
    {
      if (ptr == default || length <= 0)
      {
        Ptr = default;
        Length = default;
        return;
      }
      Ptr = ptr;
      Length = length;
    }

    public ref T this[long index] => ref Ptr[index];

    public Enumerator GetEnumerator() => new Enumerator(this);
    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  }
}