using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace AAA
{
    public static class UnityPackageBuilder
    {
        [RuntimeInitializeOnLoadMethod]
        public static void AAA()
        {
            using (var writer = File.CreateText("/opt/BBB.txt"))
                writer.WriteLine("Hello, world\n" + Guid.NewGuid().ToString());
        }
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