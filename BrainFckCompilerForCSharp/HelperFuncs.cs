// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Collections.Generic;
using System;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// Some Extension methods that get used everywhere in the compiler.
    /// </summary>
    public static class HelperFuncs
    {
        /// <summary>
        /// Checks if <paramref name="code"/> Assigns a value.
        /// </summary>
        /// <param name="code">The code to check.</param>
        /// <returns><c>true</c> if <paramref name="code"/> Assigns a value <c>false otherwise</c></returns>
        public static bool AssignsValue(this OpCode code) => code == OpCode.GetInput || code == OpCode.AssignVal;

        /// <summary>
        /// Gets the reverse <see cref="OpCode"/> of <paramref name="code"/>
        /// </summary>
        /// <param name="code">The <see cref="OpCode"/> to get the reverse of.</param>
        /// <returns>
        /// the reverse <see cref="OpCode"/> of <paramref name="code"/> if it exists otherwise <see cref="OpCode.Nop"/>
        /// </returns>
        public static OpCode GetReversedOpCode(this OpCode code)
        {
            switch (code)
            {
                case OpCode.AddVal:
                    return OpCode.SubVal;

                case OpCode.SubVal:
                    return OpCode.AddVal;

                case OpCode.AddPtr:
                    return OpCode.SubPtr;

                case OpCode.SubPtr:
                    return OpCode.AddPtr;

                default:
                    return OpCode.Nop;
            }
        }

        /// <summary>
        /// Checks if <paramref name="code"/> is reverseable.
        /// </summary>
        /// <param name="code">The code to check.</param>
        /// <returns><c>true</c> if <paramref name="code"/> is reverseable <c>false otherwise</c></returns>
        public static bool IsReversable(this OpCode code) =>
            code == OpCode.AddVal || code == OpCode.SubVal || code == OpCode.AddPtr || code == OpCode.SubPtr;

        /// <summary>
        /// Checks if <paramref name="code"/> Modifies a value.
        /// </summary>
        /// <param name="code">The code to check.</param>
        /// <returns><c>true</c> if <paramref name="code"/> Modifies a value <c>false otherwise</c></returns>
        public static bool ModifiesValue(this OpCode code) => code == OpCode.AddVal || code == OpCode.SubVal;

        /// <summary>
        /// Removes all <see cref="Instruction"/> with the opcode <see cref="OpCode.Nop"/> in
        /// <paramref name="list"/>
        /// </summary>
        /// <param name="list">A list of instructions, probably for being compiled...</param>
        public static void RemoveNops(this List<Instruction> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].OpCode == OpCode.Nop)
                {
                    list.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Converts a given char <paramref name="c"/> to a <see cref="Instruction"/> certain
        /// instructions use <paramref name="value"/>
        /// </summary>
        /// <param name="c">The character to convert to IL.</param>
        /// <param name="value">
        /// the <see cref="Instruction.Value"/> of the generated instruction if it needs it.
        /// </param>
        /// <returns>c as a new <see cref="Instruction"/>.</returns>
        [Obsolete("This and all related methods have been replaced by an Abstract Syntax Tree, " +
            "and will be removed in a future version.")]
        public static Instruction ToIL(this char c, byte value)
        {
            switch (c)
            {
                case '+':
                    return new Instruction((value > 0) ? OpCode.AddVal : OpCode.Nop, value);

                case '-':
                    return new Instruction((value > 0) ? OpCode.SubVal : OpCode.Nop, value);

                case '>':
                    return new Instruction((value > 0) ? OpCode.AddPtr : OpCode.Nop, value);

                case '<':
                    return new Instruction((value > 0) ? OpCode.SubPtr : OpCode.Nop, value);

                case ',':
                    return new Instruction(OpCode.GetInput, 0);

                case '.':
                    return new Instruction(OpCode.SetOutput, 0);

                case '[':
                    return new Instruction(OpCode.StartLoop, 0);

                case ']':
                    return new Instruction(OpCode.EndLoop, 0);

                default:
                    return new Instruction(OpCode.Nop);
            }
        }

        /// <summary>
        /// Checks if <paramref name="start"/> and <paramref name="end"/> form a loop.
        /// </summary>
        /// <param name="start">The OpCode of the first instruction.</param>
        /// <param name="end">The OpCode of the second instruction.</param>
        /// <returns>
        /// True if <paramref name="start"/> and <paramref name="end"/> form a loop, false otherwise.
        /// </returns>
        internal static bool IsIlLoop(OpCode start, OpCode end) => start == OpCode.StartLoop && end == OpCode.EndLoop;
    }
}
