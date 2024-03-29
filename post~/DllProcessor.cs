﻿using System;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
internal struct DllProcessor : IDisposable
{
  private readonly ModuleDefinition mainModule;
  private readonly FileInfo outputDll;

  public DllProcessor(FileInfo input, FileInfo output)
  {
    mainModule = ModuleDefinition.ReadModule(input.FullName);
    outputDll = output;
  }

  public void Process()
  {
    ProcessEachMethod(RewriteUnsafeAsRef);
    mainModule.Types.Remove(mainModule.GetType("UniNativeLinq", "Unsafe"));
    ProcessEachMethod(RewriteThrowNotImplementedException, PredicateThrowNotImplementedException);
  }

  private void ProcessEachMethod(Action<MethodDefinition> action, Func<TypeDefinition, bool> predicate = default)
  {
    foreach (TypeDefinition typeDefinition in mainModule.Types)
      ProcessEachMethod(action, predicate, typeDefinition);
  }

  private void ProcessEachMethod(Action<MethodDefinition> action, Func<TypeDefinition, bool> predicate, TypeDefinition typeDefinition)
  {
    foreach (TypeDefinition nestedTypeDefinition in typeDefinition.NestedTypes)
      ProcessEachMethod(action, predicate, nestedTypeDefinition);
    if (predicate is null || predicate(typeDefinition))
      foreach (MethodDefinition methodDefinition in typeDefinition.Methods)
        action(methodDefinition);
  }

  private void RewriteUnsafeAsRef(MethodDefinition methodDefinition)
  {
    Mono.Collections.Generic.Collection<Instruction> instructions;
    try
    {
      instructions = methodDefinition.Body.Instructions;
    }
    catch (NullReferenceException)
    {
      return;
    }
    catch
    {
      Console.WriteLine(methodDefinition.FullName);
      throw;
    }
    for (int i = instructions.Count - 1; i >= 0; i--)
    {
      Instruction instruction = instructions[i];
      if (instruction.OpCode.Code != Code.Call) continue;
      MethodDefinition callMethodDefinition;
      try
      {
        callMethodDefinition = ((MethodReference)instruction.Operand).ToDefinition();
      }
      catch
      {
        continue;
      }
      if (callMethodDefinition.Name != "AsRef" || callMethodDefinition.DeclaringType.Name != "Unsafe") continue;
      instructions.RemoveAt(i);
    }
  }

  private bool PredicateThrowNotImplementedException(TypeDefinition typeDefinition)
  {
    if (!typeDefinition.HasFields) return false;
    return typeDefinition.Fields.Any(field => !field.IsStatic && field.Name == "element");
  }

  private void RewriteThrowNotImplementedException(MethodDefinition methodDefinition)
  {
    if (methodDefinition.IsStatic) return;
    FieldReference elementFieldReference = methodDefinition.DeclaringType.FindField("element").MakeHostInstanceGeneric(methodDefinition.DeclaringType.GenericParameters);
    ILProcessor processor = methodDefinition.Body.GetILProcessor();
    Mono.Collections.Generic.Collection<Instruction> instructions = methodDefinition.Body.Instructions;
    for (int i = instructions.Count - 1; i >= 0; i--)
    {
      Instruction throwInstruction = instructions[i];
      if (throwInstruction.OpCode.Code != Code.Throw) continue;
      Instruction newObjInstruction = instructions[i - 1];
      if (newObjInstruction.OpCode.Code != Code.Newobj) continue;
      MethodDefinition newObjMethodDefinition;
      try
      {
        newObjMethodDefinition = ((MethodReference)newObjInstruction.Operand).ToDefinition();
      }
      catch
      {
        continue;
      }
      if (newObjMethodDefinition.Name != ".ctor" || newObjMethodDefinition.DeclaringType.FullName != "System.NotImplementedException") continue;
      newObjInstruction.Replace(Instruction.Create(OpCodes.Ldarg_0));
      throwInstruction.Replace(Instruction.Create(OpCodes.Ldflda, elementFieldReference));
      processor.InsertAfter(throwInstruction, Instruction.Create(OpCodes.Ret));
    }
  }

  public void Dispose()
  {
    using (Stream writer = new FileStream(outputDll.FullName, FileMode.Create, FileAccess.Write))
    {
      mainModule.Assembly.Write(writer);
    }
    mainModule.Dispose();
  }
}