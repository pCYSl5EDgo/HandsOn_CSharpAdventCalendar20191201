using System.Collections.Generic;

namespace UniNativeLinq
{
  public interface IRefEnumerable<TEnumerator, T> : IEnumerable<T>
    where TEnumerator : IRefEnumerator<T>
  {
    new TEnumerator GetEnumerator();
  }
}