namespace UniNativeLinq
{
  public static unsafe class NativeEnumerable
  {
    public static NativeEnumerable<T> AsRefEnumerable<T>(this Unity.Collections.NativeArray<T> array)
      where T : unmanaged
      => new NativeEnumerable<T>(ptr: (T*)Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(array), length: array.Length);
  }
}