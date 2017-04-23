// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.IO;

public static class UIHandler
{
    public static void SubscribeEvents()
    {
        Program.OnOpen += UserEliminatesRedundentCode;
        Program.OnOpen += UserEliminatesRepeatedInput;
        Program.OnOpen += UserInputsFromFile;
        Program.OnCompile += GetUserCode;
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

    private static void UserInputsFromFile()
    {
        Console.Clear();
        Console.WriteLine("Input from file? y/n");

        Settings.GetCodeFromFile = GetYnInput();
    }

    private static void UserEliminatesRedundentCode()
    {
        Console.Clear();
        Console.WriteLine("Eliminate redundencies? e.x ++- turns into + or ><< turns into < y/n?");

        Settings.EliminateRedundentCode = GetYnInput();
    }

    private static void UserEliminatesRepeatedInput()
    {
        Console.Clear();
        Console.WriteLine("Eliminate repeated inputs (,,,,, turns into , because you aren't doing anything with it before changing it) y/n?");
        Settings.EliminateRepeatedInput = GetYnInput();
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
