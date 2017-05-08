// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Collections.Generic;

public static class HelperFuncs
{
    public static void RemoveNoOps(this List<Instruction> ls)
    {
        for (int i = ls.Count - 1; i >= 0; i--)
        {
            if (ls[i].OpCode == OpCode.NoOp)
            {
                ls.RemoveAt(i);
            }
        }
    }

    public static OpCode GetReversedCode(this OpCode code)
    {
        if (code == OpCode.AddVal)
        {
            return OpCode.SubVal;
        }
        if (code == OpCode.SubVal)
        {
            return OpCode.AddVal;
        }
        if (code == OpCode.AddPtr)
        {
            return OpCode.SubPtr;
        }
        if (code == OpCode.SubPtr)
        {
            return OpCode.AddPtr;
        }
        return OpCode.NoOp;
    }

    public static bool IsReversable(this OpCode code)
    {
        return code == OpCode.AddVal || code == OpCode.SubVal || code == OpCode.AddPtr || code == OpCode.SubPtr;
    }

    public static bool AssignsValue(this OpCode code)
    {
        return code == OpCode.GetInput || code == OpCode.AssignVal;
    }

    public static bool ModifiesValue(this OpCode code)
    {
        return code == OpCode.AddVal || code == OpCode.SubVal;
    }
}
