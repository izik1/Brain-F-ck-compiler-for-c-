// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Collections.Generic;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// Optimizes the given IL to be faster and smaller.
    /// </summary>
    internal static class Optimizer
    {
        /// <summary>
        /// Uses <paramref name="settings"/> to optimize <paramref name="IL"/>.
        /// </summary>
        /// <param name="IL">The IL that is to be optimized.</param>
        /// <param name="settings">
        /// The compiler settings that tell which optimizations should be used.
        /// </param>
        internal static void Optimize(List<Instruction> IL, CompilerSettings settings)
        {
            int CodeLength = 0;

            // It feels evil to use a do while loop but this seems to be the best way to not loop
            // more than required.
            do
            {
                CodeLength = IL.Count;
                if (settings.EliminateRedundentCode)
                {
                    EliminateRedundency(IL);
                }

                if (settings.SimplifyAssignZeroLoops)
                {
                    SimplifyAssignZeroLoops(IL);
                }

                if (settings.EliminateEmptyLoops)
                {
                    EliminateEmptyLoops(IL);
                }

                if (settings.EliminateUnreachableLoops)
                {
                    EliminateUnreachableLoops(IL);
                }
            } while (IL.Count < CodeLength);

            // This one gets it's own loop because it doesn't help the other ones, less time spent here.
            if (settings.EliminateDeadStores)
            {
                do
                {
                    CodeLength = IL.Count;
                    EliminateDeadStores(IL);
                } while (IL.Count < CodeLength);
            }

            // Doesn't need a loop.
            if (settings.MergeAssignThenModifyInstructions)
            {
                MergeAssignThenModifyInstructions(IL);
            }
        }

        /// <summary>
        /// Eliminate changes to values that get overridden.
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        internal static void EliminateDeadStores(List<Instruction> IL)
        {
            for (int i = 1; i < IL.Count; i++)
            {
                if (IsDeadStore(IL[i - 1].OpCode, IL[i].OpCode))
                {
                    IL[i - 1].Invalidate();
                }
            }

            IL.RemoveNoOps();
        }

        /// <summary>
        /// Removes all loops that contain nothing ( <see cref="OpCode.StartLoop"/> followed by a
        /// <see cref="OpCode.EndLoop"/>.)
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        internal static void EliminateEmptyLoops(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (HelperFuncs.IsIlLoop(IL[i].OpCode, IL[i + 1].OpCode))
                {
                    IL[i + 1].Invalidate();
                    IL[i].Invalidate();
                }
            }

            IL.RemoveNoOps();
        }

        /// <summary>
        /// Eliminates redundent instructions, for instance add 1 then subtract 5 gets replaced with
        /// subtract 4
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        internal static void EliminateRedundency(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (IL[i].OpCode.IsReversable()) // The following if and else if both would check for this.
                {
                    if (IL[i + 1].OpCode == IL[i].OpCode)
                    {
                        // Combine the two instructions and Invalidate the 2nd one because it gets
                        // removed faster when List.Remove is called.
                        IL[i].Value += IL[i + 1].Value;
                        IL[i + 1].Invalidate();
                    }
                    else if (IL[i + 1].OpCode == IL[i].OpCode.GetReversedCode())
                    {
                        if (IL[i + 1].Value > IL[i].Value)
                        {
                            IL[i + 1].Value -= IL[i].Value;
                            IL[i].Invalidate();
                        }
                        else if (IL[i + 1].Value < IL[i].Value)
                        {
                            IL[i].Value -= IL[i + 1].Value;
                            IL[i + 1].Invalidate();
                        }
                        else
                        {
                            IL[i].Invalidate();
                            IL[i + 1].Invalidate();
                        }
                    }
                    else
                    {
                        // Do nothing as this is a normal case but nothing needs to be done.
                    }
                }
            }

            IL.RemoveNoOps();
        }

        /// <summary>
        /// Removes all loops that are unreachable (meaning that no matter what they can't run)
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        internal static void EliminateUnreachableLoops(List<Instruction> IL)
        {
            Stack<int> LoopEnds = new Stack<int>();

            // Not sure if this local function should exist.
            bool DetectUnreachableLoops(int current)
            {
                return IL[current + 1].OpCode == OpCode.StartLoop &&
                    ((IL[current].OpCode == OpCode.AssignVal && IL[current].Value == 0) || current == 0);
            }
            for (int i = IL.Count - 1; i >= 0; i--)
            {
                if (IL[i].OpCode == OpCode.EndLoop)
                {
                    LoopEnds.Push(i);
                }

                // If MostRecentLoopEnd is less than 1 no loop can made.
                else if (LoopEnds.Count > 0 && DetectUnreachableLoops(i))
                {
                    int MostRecentLoopEnd = LoopEnds.Pop();
                    for (int j = i; j < MostRecentLoopEnd + 1; j++)
                    {
                        IL[j].Invalidate();
                    }
                }
                else
                {
                    // Do nothing.
                }
            }

            IL.RemoveNoOps();
        }

        /// <summary>
        /// Checks if <paramref name="current"/> gets overridden by <paramref name="next"/>.
        /// </summary>
        /// <param name="current">The OpCode of the current instruction.</param>
        /// <param name="next">The OpCode of the next instruction.</param>
        /// <returns>
        /// True if <paramref name="current"/> gets overridden by <paramref name="next"/>, false otherwise.
        /// </returns>
        private static bool IsDeadStore(OpCode current, OpCode next) =>
            next.AssignsValue() && (current.ModifiesValue() || current.AssignsValue());

        /// <summary>
        /// Merges all <see cref="Instruction"/> that have <see cref="OpCode.AssignVal"/> with ones
        /// that add or subtract to the value.
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        // This name is stupidly long but at least it is descriptive, still coding at 2:55
        internal static void MergeAssignThenModifyInstructions(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (IL[i].OpCode == OpCode.AssignVal)
                {
                    if (IL[i + 1].OpCode == OpCode.AddVal)
                    {
                        IL[i].Value += IL[i + 1].Value;
                        IL[i + 1].Invalidate();
                    }
                    else if (IL[i + 1].OpCode == OpCode.SubVal)
                    {
                        IL[i].Value -= IL[i + 1].Value;
                        IL[i + 1].Invalidate();
                    }
                    else
                    {
                        // Do nothing.
                    }
                }
            }

            IL.RemoveNoOps();
        }

        /// <summary>
        /// Simplifies loops that assign a value to zero.
        /// </summary>
        /// <param name="IL">The il.</param>
        internal static void SimplifyAssignZeroLoops(List<Instruction> IL)
        {
            for (int i = IL.Count - 3; i >= 0; i--)
            {
                if (HelperFuncs.IsIlLoop(IL[i].OpCode, IL[i + 2].OpCode))
                {
                    if (IL[i + 1].OpCode == OpCode.AssignVal && IL[i + 1].Value == 0)
                    {
                        IL[i + 0].Invalidate();
                        IL[i + 2].Invalidate();
                    }

                    // As it turns out, all odd numbers are valid, not just 1.
                    else if (IL[i + 1].OpCode.ModifiesValue() && IL[i + 1].Value % 2 != 0)
                    {
                        IL[i + 0] = new Instruction(OpCode.AssignVal, 0);
                        IL[i + 1].Invalidate();
                        IL[i + 2].Invalidate();
                    }
                    else
                    {
                        // Do nothing.
                    }
                }
            }

            IL.RemoveNoOps();
        }
    }
}
