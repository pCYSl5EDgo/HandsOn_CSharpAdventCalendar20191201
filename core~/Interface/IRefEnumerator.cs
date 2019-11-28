using System.Collections.Generic;

namespace UniNativeLinq
{
  public interface IRefEnumerator<T> : IEnumerator<T>
  {
    new ref T Current { get; }
  }
}