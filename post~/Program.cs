using System;
using System.IO;
public sealed class Program
{
  static int Main(string[] args)
  {
    if (!ValidateArguments(args, out FileInfo inputUniNativeLinqDll, out FileInfo outputUniNativeLinqDllPath))
    {
      return 1;
    }
    using (DllProcessor processor = new DllProcessor(inputUniNativeLinqDll, outputUniNativeLinqDllPath))
    {
      processor.Process();
    }
    return 0;
  }

  private static bool ValidateArguments(string[] args, out FileInfo inputUniNativeLinqDll, out FileInfo outputNativeLinqDllPath)
  {
    if (args.Length != 2)
    {
      Console.Error.WriteLine("Invalid argument count.");
      inputUniNativeLinqDll = default;
      outputNativeLinqDllPath = default;
      return false;
    }
    inputUniNativeLinqDll = new FileInfo(args[0]);
    if (!inputUniNativeLinqDll.Exists)
    {
      Console.Error.WriteLine("Empty Input UniNativeLinq.dll path");
      outputNativeLinqDllPath = default;
      return false;
    }
    string outputNativeLinqDllPathString = args[1];
    if (string.IsNullOrWhiteSpace(outputNativeLinqDllPathString))
    {
      Console.Error.WriteLine("Empty Output UniNativeLinq.dll path");
      outputNativeLinqDllPath = default;
      return false;
    }
    outputNativeLinqDllPath = new FileInfo(outputNativeLinqDllPathString);
    return true;
  }
}