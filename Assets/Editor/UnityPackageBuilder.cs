using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
namespace HandsOn
{
  public static class UnityPackageBuilder
  {
    public static void Build()
    {
      Debug.Log("BUILD=LOCATION\n" + Assembly.GetExecutingAssembly().Location + "\n" + Environment.StackTrace);
      var args = Environment.GetCommandLineArgs();
      var exportPath = args[args.Length - 1];
      AssetDatabase.ExportPackage(
      new[]{
          "Assets/Plugins/UNL/UniNativeLinq.dll"
      },
      exportPath,
      ExportPackageOptions.Default
      );
    }
  }
}