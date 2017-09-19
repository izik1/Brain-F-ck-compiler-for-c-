// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Collections.Generic;
using System.Linq;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// Optimizes the given IL to be faster and smaller.
    /// </summary>
    internal static class Optimizer
    {
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

            IL.RemoveNops();
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

            IL.RemoveNops();
        }

        /// <summary>
        /// Eliminates redundent instructions, for instance add 1 then subtract 5 gets replaced with
        /// subtract 4
        /// </summary>
        /// <param name="IL">The IL code to be optimized.</param>
        private static void CombineMatchingInstructions(List<Instruction> IL)
        {
            for (int i = IL.Count - 2; i >= 0; i--)
            {
                if (IL[i].OpCode.IsReversable() && IL[i + 1].OpCode == IL[i].OpCode)
                {
                    // Combine the two instructions and Invalidate the 2nd one because it gets
                    // removed faster when List.Remove is called.
                    IL[i].Value += IL[i + 1].Value;
                    IL[i + 1].Invalidate();
                }
            }

            IL.RemoveNops();
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

            IL.RemoveNops();
        }

        private static bool EliminateUnreachableLoops(AbstractSyntaxTree root, bool trueRoot)
        {
            bool RetVal = false;
            for (int i = 1; i < root.Count; i++)
            {
                RetVal |= EliminateUnreachableLoops(root[i], false);
                if (root[i].Op == OpCode.Loop && (root[i - 1].Op == OpCode.Loop || root[i - 1].Op == OpCode.AssignZero))
                {
                    root.RemoveAt(i);
                    RetVal = true;
                    i--;
                }
            }

            if (trueRoot)
            {
                if (root.Count > 0 && root[0].Op == OpCode.Loop)
                {
                    root.RemoveAt(0);
                    return true;
                }

                if (root.Count == 0 && root.Op == OpCode.Loop)
                {
                    root.Op = OpCode.Nop;
                    return true;
                }
            }

            return RetVal;
        }

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

            IL.RemoveNops();
        }

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
                if (settings.CombineMatchingInstructions)
                {
                    CombineMatchingInstructions(IL);
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

        internal static void Optimize(AbstractSyntaxTree ast, CompilerSettings settings)
        {
            bool Continue = true;
            while (Continue)
            {
                Continue = false;
                if (settings.SimplifyAssignZeroLoops)
                {
                    Continue |= SimplifyAssignZeroLoops(ast);
                    ast.ColapseNops();
                }

                if (settings.EliminateDeadStores)
                {
                    Continue |= EliminateDeadStores(ast, true);
                    ast.ColapseNops();
                }

                if (settings.EliminateUnreachableLoops)
                {
                    Continue |= EliminateUnreachableLoops(ast, true);
                    ast.ColapseNops();
                }

                if (settings.EliminateConflictingInstructions) // TODO: replace with a setting.
                {
                    Continue |= EliminateConflictingInstructions(ast);
                }
            }
        }

        internal static bool EliminateConflictingInstructions(AbstractSyntaxTree ast)
        {
            bool RetVal = false;
            if (ast.Count > 0)
            {
                RetVal |= EliminateConflictingInstructions(ast[0]);
            }

            for (int i = 1; i < ast.Count; i++)
            {
                RetVal |= EliminateConflictingInstructions(ast[i]);
                if (ast[i - 1].Op.IsReversable() && ast[i - 1].Op.GetReversedOpCode() == ast[i].Op)
                {
                    ast.RemoveAt(i);
                    ast.RemoveAt(i - 1);
                    i += 2;
                    RetVal = true;
                }
            }

            return RetVal;
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

            IL.RemoveNops();
        }

        private static bool EliminateDeadStores(AbstractSyntaxTree root, bool trueRoot)
        {
            bool RetVal = false;
            for (int i = root.Count - 2; i >= 0; i--)
            {
                RetVal |= EliminateDeadStores(root[i], false);
                if (root[i + 1].Op == OpCode.AssignZero || root[i + 1].Op == OpCode.AssignVal)
                {
                    AbstractSyntaxTree ast = root[i];
                    if (ast.Op.AssignsValue() || ast.Op == OpCode.AssignZero || ast.Op.ModifiesValue())
                    {
                        root.Remove(ast);
                    }
                }
            }

            if (root.Count > 0 && root[0].Op == OpCode.AssignZero && trueRoot)
            {
                root.RemoveAt(0);
                return true;
            }

            return RetVal;
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

        private static bool SimplifyAssignZeroLoops(AbstractSyntaxTree root)
        {
            bool RetVal = false;

            bool Predicate(AbstractSyntaxTree ast) =>
                (ast.AllChildrenAssignZero || ast.AllChildrenDecVal || ast.AllChildrenIncVal) && ast.Count % 2 != 0;

            // ToList hack allows the original list to be changed.
            foreach (AbstractSyntaxTree ast in root.ChildNodes)
            {
                RetVal |= SimplifyAssignZeroLoops(ast);

                if (ast.Op == OpCode.Loop && Predicate(ast))
                {
                    OpCode op = ast[0].Op;
                    if (op == OpCode.AssignZero || op.ModifiesValue())
                    {
                        ast.ChildNodes.RemoveAt(0);
                        ast.Op = OpCode.AssignZero;
                        RetVal = true;
                    }
                }
            }

            return RetVal;
        }
    }
}
