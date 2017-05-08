﻿// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Collections.Generic;

public static class Optimizer
{
    public static void Optimize(List<Instruction> codeIn)
    {
        List<Instruction> code = codeIn;
        if (Settings.SimplifyToZeroLoops)
        {
            SimplifyToZeroLoops(code);
        }
        int CodeLength = 0;
        do
        {
            CodeLength = code.Count;
            if (Settings.EliminateRedundentCode)
            {
                EliminateRedundency(code);
            }
            if (Settings.EliminateRepeatedFlatValues)
            {
                EliminateRepeatedFlatValue(code);
            }
        } while (code.Count < CodeLength);
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

    private static void EliminateRepeatedFlatValue(List<Instruction> code)
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
