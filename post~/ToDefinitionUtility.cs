using Mono.Cecil;
internal static class ToDefinitionUtility
{
  public static TypeDefinition ToDefinition(this TypeReference reference) => reference switch
  {
    TypeDefinition definition => definition,
    GenericInstanceType generic => generic.ElementType.ToDefinition(),
    _ => reference.Resolve(),
  };
  public static MethodDefinition ToDefinition(this MethodReference reference) => reference switch
  {
    MethodDefinition definition => definition,
    GenericInstanceMethod generic => generic.ElementMethod.ToDefinition(),
    _ => reference.Resolve(),
  };
}