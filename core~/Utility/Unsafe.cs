using System;

namespace UniNativeLinq
{
  public static class Unsafe
  {
    public static ref T AsRef<T>(in T value) => throw new NotImplementedException();
  }
}