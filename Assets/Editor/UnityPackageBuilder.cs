using System;
using UnityEditor;
namespace AAA
{
    public static class UnityPackageBuilder
    {
        public static void Build()
        {
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