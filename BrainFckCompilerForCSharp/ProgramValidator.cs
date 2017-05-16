// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Linq;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// </summary>
    internal static class ProgramValidator
    {
        /// <summary>
        /// Ensures that <paramref name="IL"/> is valid program code.
        /// </summary>
        /// <param name="IL"></param>
        /// <returns></returns>
        internal static (bool ValidProgram, string OutputText) Validate(List<Instruction> IL)
        {
            if (NullProgram(IL))
            {
                return (false, "Null program");
            }
            if (!LoopsBalanced(IL))
            {
                return (false, "Loops aren't balanced ( [[] ) or maybe loops are out of order ( ][ ) ");
            }
            if (!ValidLoops(IL))
            {
                return (false, "You have an infinite loop somewhere.");
            }
            return (true, "Valid program");
        }

        /// <summary>
        /// Checks to see if <paramref name="IL"/> has balanced loops
        /// </summary>
        /// <param name="IL"></param>
        /// <returns></returns>
        private static bool LoopsBalanced(List<Instruction> IL)
        {
            if (IL.Count(c => c.OpCode == OpCode.StartLoop) != IL.Count(c => c.OpCode == OpCode.EndLoop))
            {
                return false;
            }
            int counter = 0;
            for (int i = 0; i < IL.Count; i++)
            {
                if (IL[i].OpCode == OpCode.StartLoop)
                {
                    counter++;
                }
                else if (IL[i].OpCode == OpCode.EndLoop)
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

        /// <summary>
        /// Makes sure that there aren't any obvious infinite loops.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static bool ValidLoops(List<Instruction> code)
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

        /// <summary>
        /// Self explanitory, makes sure <paramref name="code"/> has at least one element.
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static bool NullProgram(List<Instruction> code)
        {
            return code.Count == 0;
        }
    }
}
