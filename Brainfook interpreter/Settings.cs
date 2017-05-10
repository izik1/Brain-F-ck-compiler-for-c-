// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

public static class Settings
{
    public static bool EliminateRedundentCode { get; set; }

    public static bool EliminateDeadStores { get; set; }

    public static bool SimplifyToZeroLoops { get; set; } // Basically loops that set the value at the current pointer to 0.

    public static bool GetCodeFromFile { get; set; }

    public static bool MergeAssignThenModifyInstructions { get; set; }

    public static string InputCode { get; set; }
}
