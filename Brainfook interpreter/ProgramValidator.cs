// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ProgramValidator
{
    public static bool Validate(string code)
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
