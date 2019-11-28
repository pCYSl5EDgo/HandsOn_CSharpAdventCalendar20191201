using System.Collections;
using System.Collections.Generic;

namespace UniNativeLinq
{
  public readonly partial struct NativeEnumerable<T>
  {
    public unsafe struct Enumerator : IRefEnumerator<T>
    {
      private readonly T* ptr;
      private readonly long length;
      private long index;

      public Enumerator(NativeEnumerable<T> parent)
      {
        ptr = parent.Ptr;
        length = parent.Length;
        index = -1;
      }

      public bool MoveNext() => ++index < length;
      public void Reset() => index = -1;
      public ref T Current => ref ptr[index];
      T IEnumerator<T>.Current => Current;
      object IEnumerator.Current => Current;
      public void Dispose() => this = default;
    }
  }
}