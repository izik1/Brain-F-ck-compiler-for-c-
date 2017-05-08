// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

/// <summary>
/// </summary>
public enum OpCode
{
    /// <summary>
    /// No Op stands for No Operation, no code is ran with these, but it's useful to mark
    /// instructions to be removed. Set to zero for semantics.
    /// </summary>
    NoOp = 0,

    AddVal,
    SubVal,
    AddPtr,
    SubPtr,
    GetInput,
    SetOutput,
    StartLoop,
    EndLoop,
    AssignVal
}
