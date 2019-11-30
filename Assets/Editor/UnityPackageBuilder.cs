using System;
using UnityEditor;

public static class UnityPackageBuilder
{
    public static void Build()
    {
        const string exportPath = "../UniNativeLinq.unitypackage";
        AssetDatabase.ExportPackage(
           new []{
               "Assets/Plugins/UNL/UniNativeLinq.dll"
           },
           exportPath,
           ExportPackageOptions.Default
        );
    }
}