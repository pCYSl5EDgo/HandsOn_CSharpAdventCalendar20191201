namespace UniNativeLinq
{
  public readonly partial struct SelectEnumerable<TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction>
    : IRefEnumerable<SelectEnumerable<TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction>.Enumerator, T>
    where TPrevEnumerable : IRefEnumerable<TPrevEnumerator, TPrev>
    where TPrevEnumerator : IRefEnumerator<TPrev>
    where TAction : IRefAction<TPrev, T>
  {
    private readonly TPrevEnumerable enumerable;
    private readonly TAction action;

    public SelectEnumerable(in TPrevEnumerable enumerable)
    {
      this.enumerable = enumerable;
      action = default;
    }

    public SelectEnumerable(in TPrevEnumerable enumerable, in TAction action)
    {
      this.enumerable = enumerable;
      this.action = action;
    }

    public Enumerator GetEnumerator() => new Enumerator(ref Unsafe.AsRef(enumerable), action);
    System.Collections.Generic.IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator() => GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
  }
}