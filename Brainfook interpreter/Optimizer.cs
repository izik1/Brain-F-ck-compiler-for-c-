// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

public static class Optimizer
{
    public static string Optimize(string codeIn)
    {
        string code = codeIn;
        if (Settings.EliminateRedundentCode)
        {
            while (RedundencyEliminator.Eliminate(code).Length < code.Length)
            {
                code = RedundencyEliminator.Eliminate(code);
            };
        }

        if (Settings.EliminateRepeatedInput)
        {
            code = RedundencyEliminator.Eliminate(codeIn);
        }

        return code;
    }
}
