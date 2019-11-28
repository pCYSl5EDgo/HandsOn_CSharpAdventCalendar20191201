namespace UniNativeLinq
{
  public readonly partial struct SelectEnumerable<TPrevEnumerable, TPrevEnumerator, TPrev, T, TAction>
  {
    public struct Enumerator : IRefEnumerator<T>
    {
      private TPrevEnumerator enumerator;
      private TAction action;
      private T element;

      public Enumerator(ref TPrevEnumerable enumerable, in TAction action)
      {
        enumerator = enumerable.GetEnumerator();
        this.action = action;
        element = default;
      }

      public bool MoveNext()
      {
        if (!enumerator.MoveNext()) return false;
        action.Execute(ref enumerator.Current, ref element);
        return true;
      }

      public void Reset() => throw new System.InvalidOperationException();
      public ref T Current => throw new System.NotImplementedException();
      T System.Collections.Generic.IEnumerator<T>.Current => Current;
      object System.Collections.IEnumerator.Current => Current;
      public void Dispose() { }
    }
  }
}
