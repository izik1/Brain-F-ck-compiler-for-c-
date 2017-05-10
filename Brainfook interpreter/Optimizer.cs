// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Collections.Generic;

public static class Optimizer
{
    public static void Optimize(List<Instruction> code)
    {
        int CodeLength = 0;
        do
        {
            CodeLength = code.Count;
            if (Settings.EliminateRedundentCode)
            {
                EliminateRedundency(code);
            }

            if (Settings.SimplifyToZeroLoops)
            {
                SimplifyToZeroLoops(code);
            }
            if (Settings.EliminateEmptyLoops)
            {
                EliminateEmptyLoops(code);
            }
        } while (code.Count < CodeLength);

        // Coding at 2:45, fairly certain this should be out of the other loop because it doesn't
        // affect the other things.
        if (Settings.EliminateDeadStores)
        {
            do
            {
                CodeLength = code.Count;
                EliminateDeadStores(code);
            } while (code.Count < CodeLength);
        }
        if (Settings.MergeAssignThenModifyInstructions)
        {
            MergeAssignThenModifyInstructions(code);
        }
    }

    private static void EliminateEmptyLoops(List<Instruction> code)
    {
        for (int i = code.Count - 2; i >= 0; i--)
        {
            if (code[i + 1].OpCode == OpCode.EndLoop && code[i].OpCode == OpCode.StartLoop)
            {
                code[i + 1].Invalidate();
                code[i].Invalidate();
            }
        }
        code.RemoveNoOps();
    }

    // This name is stupidly long but at least it is descriptive, still coding at 2:55
    private static void MergeAssignThenModifyInstructions(List<Instruction> code)
    {
        for (int i = code.Count - 2; i >= 0; i--)
        {
            if (code[i].OpCode == OpCode.AssignVal)
            {
                if (code[i + 1].OpCode == OpCode.AddVal)
                {
                    code[i].Value += code[i + 1].Value;
                    code[i + 1].Invalidate();
                }
                else if (code[i + 1].OpCode == OpCode.SubVal)
                {
                    code[i].Value -= code[i + 1].Value;
                    code[i + 1].Invalidate();
                }
                else
                {
                    // Do nothing.
                }
            }
        }
        code.RemoveNoOps();
    }

    private static void EliminateRedundency(List<Instruction> code)
    {
        for (int i = code.Count - 2; i >= 0; i--)
        {
            if (code[i + 1].OpCode == code[i].OpCode && code[i].OpCode.IsReversable())
            {
                code[i].Value += code[i + 1].Value;
                code[i + 1].Invalidate();
            }
            else if (code[i + 1].OpCode == code[i].OpCode.GetReversedCode() && code[i].OpCode.IsReversable())
            {
                if (code[i + 1].Value > code[i].Value)
                {
                    code[i + 1].Value -= code[i].Value;
                    code[i].Invalidate();
                }
                else if (code[i + 1].Value < code[i].Value)
                {
                    code[i].Value -= code[i + 1].Value;
                    code[i + 1].Invalidate();
                }
                else
                {
                    code[i].Invalidate();
                    code[i + 1].Invalidate();
                }
            }
            else
            {
                // Do nothing as this is a normal case but nothing needs to be done.
            }
        }
        code.RemoveNoOps();
    }

    private static void SimplifyToZeroLoops(List<Instruction> code)
    {
        for (int i = code.Count - 3; i >= 0; i--)
        {
            if (code[i + 2].OpCode == OpCode.EndLoop &&
                (code[i + 1].OpCode.ModifiesValue() && code[i + 1].Value == 1) &&
                code[i].OpCode == OpCode.StartLoop)
            {
                code[i + 0] = new Instruction(OpCode.AssignVal, 0);
                code[i + 1].Invalidate();
                code[i + 2].Invalidate();
            }
        }
        code.RemoveNoOps();
    }

    private static void EliminateDeadStores(List<Instruction> code)
    {
        for (int i = code.Count - 2; i >= 0; i--)
        {
            if ((code[i + 1].OpCode.AssignsValue() && (code[i].OpCode.ModifiesValue() || code[i].OpCode.AssignsValue())))
            {
                code[i].Invalidate();
            }
        }
        code.RemoveNoOps();
    }
}
