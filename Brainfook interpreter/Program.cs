// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;

public static class Program
{
    public static event Action OnCompile;

    public static event Action OnOpen;

    private static void Main()
    {
        UIHandler.SubscribeEvents();
        OnOpen();
        OnCompile();

        Console.WriteLine((Compiler.Compile(Settings.InputCode) ? "Compiled!" : "Failed to compile"));
        Console.ReadKey();
    }
}
