namespace UniNativeLinq
{
  public readonly struct DelegateRefActionToStructOperatorAction<T0, T1> : IRefAction<T0, T1>
  {
    private readonly RefAction<T0, T1> action;

    public DelegateRefActionToStructOperatorAction(RefAction<T0, T1> action) => this.action = action;
    public void Execute(ref T0 arg0, ref T1 arg1) => action(ref arg0, ref arg1);
  }
}