using System;

public static class Z
{
    public static void Hoge()
    {
       System.IO.File.WriteAllText("a.txt", "hello world");
    }
}