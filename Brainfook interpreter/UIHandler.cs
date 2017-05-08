// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.IO;

public static class UIHandler
{
    public static void SubscribeEvents()
    {
        Program.OnOpen += GetUserSettings;
        Program.OnCompile += GetUserCode;
    }

    public static void GetUserSettings()
    {
        Settings.EliminateRedundentCode = GetUserInput("Eliminate redundencies? (ex: ++- turns into + or ><< turns into <)");
        Settings.EliminateRepeatedFlatValues = GetUserInput(
            "Eliminate Repeated flat values? (ex: ,, = , or [+], = , or ,[-] = [-])");
        Settings.SimplifyToZeroLoops = GetUserInput("Simplify Loops that go to zero? ([+] or [-])");
        Settings.GetCodeFromFile = GetUserInput("Input from file?");
    }

    public static bool GetYnInput()
    {
        char c = Console.ReadKey(true).KeyChar;
        while ("YyNn".IndexOf(c) == -1)
        {
            Console.Clear();
            Console.WriteLine("Invalid input please type Y, y, N, or n");
            c = Console.ReadKey(true).KeyChar;
        }
        return (c == 'Y' || c == 'y');
    }

    public static bool GetUserInput(string displayText)
    {
        Console.Clear();
        Console.WriteLine(displayText + " y/n?");
        return GetYnInput();
    }

    private static void GetUserCode()
    {
        Console.Clear();
        if (Settings.GetCodeFromFile)
        {
            Console.WriteLine("Put code path here (note: must be a full path to an exact file)");
            string path = Console.ReadLine();
            while (!File.Exists(path))
            {
                Console.Clear();
                Console.WriteLine("Invalid path");
                path = Console.ReadLine();
            }
            Console.Clear();
            Settings.InputCode = File.ReadAllText(path);
        }
        else
        {
            Console.WriteLine("Put code here, note that it must all be on 1 line.");
            Settings.InputCode = Console.ReadLine();
        }
    }
}
