// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ProgramValidator
{
    public static bool PreOptimizeValidate(string code)
    {
        if (!CodeSanitized(code))
        {
            Console.WriteLine("Some how the code wasn't santized when it was brought in, " +
                "some hazmat suits (and some debugging on the projects end) is needed." +
                " -- you should never see this please report this if you do.");
            return false;
        }
        return PostOptimizeValidate(code);
    }

    public static bool PostOptimizeValidate(string code)
    {
        if (NullProgram(code))
        {
            Console.WriteLine($"Null Program{(Settings.EliminateRedundentCode ? ", try turning off Redundency Elimination? " : "")}");
            return false;
        }
        if (!LoopsBalanced(code))
        {
            Console.WriteLine($"Loops aren't balanced ( [[] ) or maybe loops are out of order ( ][ ) ");
            return false;
        }
        if (!ValidLoops(code))
        {
            Console.WriteLine("You have an infinite loop somewhere.");
            return false;
        }
        return true;
    }

    public static bool CodeSanitized(string code)
    {
        for (int i = 0; i < code.Length; i++)
        {
            if ("[+<,.>-]".IndexOf(code[i]) == -1)
            {
                return false;
            }
        }
        return true;
    }

    public static bool LoopsBalanced(string code)
    {
        if (code.Count(c => c == '[') != code.Count(c => c == ']'))
        {
            return false;
        }
        int counter = 0;
        for (int i = 0; i < code.Length; i++)
        {
            if (code[i] == '[')
            {
                counter++;
            }
            else if (code[i] == ']')
            {
                counter--;
            }
            else
            {
                // Do nothing :/
            }
            if (counter < 0)
            {
                return false;
            }
        }
        return true;
    }

    public static bool ValidLoops(string code)
    {
        char prev = ' ';
        for (int i = 0; i < code.Length; i++)
        {
            if (code[i] == ']' && prev == '[')
            {
                return false;
            }
            prev = code[i];
        }
        return true;
    }

    public static bool NullProgram(string code)
    {
        return code.Length == 0;
    }
}
