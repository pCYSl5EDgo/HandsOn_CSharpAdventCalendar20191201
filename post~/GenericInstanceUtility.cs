﻿using Mono.Cecil;
using System.Linq;
using System.Collections.Generic;
internal static class GenericInstanceUtility
{
  public static FieldReference FindField(this TypeReference type, string name)
  {
    if (type is TypeDefinition definition)
      return definition.FindField(name);
    if (type is GenericInstanceType genericInstanceType)
      return genericInstanceType.FindField(name);
    var typeDefinition = type.ToDefinition();
    var fieldDefinition = typeDefinition.Fields.Single(x => x.Name == name);
    if (fieldDefinition.Module == type.Module)
      return fieldDefinition;
    return type.Module.ImportReference(fieldDefinition);
  }
  public static FieldReference FindField(this TypeDefinition type, string name) => type.Fields.Single(x => x.Name == name);

  public static FieldReference FindField(this GenericInstanceType type, string name)
  {
    var typeDefinition = type.ToDefinition();
    var definition = typeDefinition.Fields.Single(x => x.Name == name);
    return definition.MakeHostInstanceGeneric(type.GenericArguments);
  }

  public static FieldReference MakeHostInstanceGeneric(this FieldReference self, IEnumerable<TypeReference> arguments) => new FieldReference(self.Name, self.FieldType, self.DeclaringType.MakeGenericInstanceType(arguments));

  public static GenericInstanceType MakeGenericInstanceType(this TypeReference self, IEnumerable<TypeReference> arguments)
  {
    var instance = new GenericInstanceType(self);
    foreach (var argument in arguments)
      instance.GenericArguments.Add(argument);
    return instance;
  }
}
