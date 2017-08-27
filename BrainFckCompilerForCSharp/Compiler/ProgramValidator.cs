// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

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
        internal static ErrorCodes Validate(List<Instruction> IL)
        {
            if (IL.Count == 0)
            {
                return ErrorCodes.NullProgram;
            }
            if (!LoopsBalanced(IL))
            {
                return ErrorCodes.ProgramContainsUnbalancedLoop;
            }
            if (!ValidLoops(IL))
            {
                return ErrorCodes.ProgramContainsInvalidLoop;
            }
            return ErrorCodes.Successful;
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
        /// <param name="IL"></param>
        /// <returns></returns>
        private static bool ValidLoops(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (HelperFuncs.IsIlLoop(IL[i].OpCode, IL[i + 1].OpCode))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
