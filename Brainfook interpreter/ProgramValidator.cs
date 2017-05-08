// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class ProgramValidator
{
    public static bool PostOptimizeValidate(List<Instruction> code)
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

    public static bool LoopsBalanced(List<Instruction> code)
    {
        if (code.Count(c => c.OpCode == OpCode.StartLoop) != code.Count(c => c.OpCode == OpCode.EndLoop))
        {
            return false;
        }
        int counter = 0;
        for (int i = 0; i < code.Count; i++)
        {
            if (code[i].OpCode == OpCode.StartLoop)
            {
                counter++;
            }
            else if (code[i].OpCode == OpCode.EndLoop)
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

    public static bool ValidLoops(List<Instruction> code)
    {
        OpCode prev = OpCode.NoOp;
        for (int i = 0; i < code.Count; i++)
        {
            if (code[i].OpCode == OpCode.StartLoop && prev == OpCode.EndLoop)
            {
                return false;
            }
            prev = code[i].OpCode;
        }
        return true;
    }

    public static bool NullProgram(List<Instruction> code)
    {
        return code.Count == 0;
    }
}
