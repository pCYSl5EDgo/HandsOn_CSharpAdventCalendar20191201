using Mono.Cecil.Cil;
internal static class InstructionUtility
{
  public static void Replace(this Instruction instruction, Instruction replace) => (instruction.OpCode, instruction.Operand) = (replace.OpCode, replace.Operand);
}