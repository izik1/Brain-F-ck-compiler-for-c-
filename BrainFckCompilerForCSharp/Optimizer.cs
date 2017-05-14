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
        private static void EliminateDeadStores(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (IsDeadStore(IL[i].OpCode, IL[i + 1].OpCode))
                {
                    IL[i].Invalidate();
                }
            }
            IL.RemoveNoOps();
        }

        /// <summary>
        /// Removes all loops that contain nothing ( <see cref="OpCode.StartLoop"/> followed by a
        /// <see cref="OpCode.EndLoop"/>.)
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        private static void EliminateEmptyLoops(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (IsLoop(IL[i].OpCode, IL[i + 1].OpCode))
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
        private static void EliminateRedundency(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (IL[i].OpCode.IsReversable()) // The following if and else if both would check for this.
                {
                    if (IL[i + 1].OpCode == IL[i].OpCode)
                    {
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
        /// Checks if <paramref name="current"/> gets overridden by <paramref name="next"/>.
        /// </summary>
        /// <param name="current">The OpCode of the current instruction.</param>
        /// <param name="next">The OpCode of the next instruction.</param>
        /// <returns>
        /// True if <paramref name="current"/> gets overridden by <paramref name="next"/>, false otherwise.
        /// </returns>
        private static bool IsDeadStore(OpCode current, OpCode next)
        {
            return next.AssignsValue() && (current.ModifiesValue() || current.AssignsValue());
        }

        /// <summary>
        /// Checks if <paramref name="first"/> and <paramref name="second"/> form a loop. ( [...] in
        /// brain f*ck or <see cref="OpCode.StartLoop"/> &lt;Any number of instructions&gt; <see
        /// cref="OpCode.EndLoop"/> in IL)
        /// </summary>
        /// <param name="first">The OpCode of the first instruction.</param>
        /// <param name="second">The OpCode of the second instruction.</param>
        /// <returns>
        /// True if <paramref name="first"/> and <paramref name="second"/> a loop, false otherwise.
        /// </returns>
        private static bool IsLoop(OpCode first, OpCode second)
        {
            return first == OpCode.StartLoop && second == OpCode.EndLoop;
        }

        /// <summary>
        /// Merges all <see cref="Instruction"/> that have <see cref="OpCode.AssignVal"/> with ones
        /// that add or subtract to the value.
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        // This name is stupidly long but at least it is descriptive, still coding at 2:55
        private static void MergeAssignThenModifyInstructions(List<Instruction> IL)
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
        /// Replaces all loops that assign 0 with a <see cref="Instruction"/> that has <see
        /// cref="Instruction.OpCode"/> = <see cref="OpCode.AssignVal"/> and <see
        /// cref="Instruction.Value"/> = 0
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        private static void SimplifyAssignZeroLoops(List<Instruction> IL)
        {
            for (int i = IL.Count - 3; i >= 0; i--)
            {
                if (IsLoop(IL[i].OpCode, IL[i + 2].OpCode))
                {
                    if ((IL[i + 1].OpCode == OpCode.AssignVal && IL[i + 1].Value == 0))
                    {
                        IL[i + 0].Invalidate();
                        IL[i + 2].Invalidate();
                    }
                    else if (IL[i + 1].OpCode.ModifiesValue() && IL[i + 1].Value == 1)
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
