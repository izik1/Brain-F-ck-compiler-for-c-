namespace BrainFckCompilerCSharp
{
    public enum ErrorCodes
    {
        Successful = 0,
        NullProgram = 1,
        ProgramContainsUnbalancedLoop = 2,
        ProgramContainsInvalidLoop = 3,
        UnknownCompilationFailure = 4,
        OutputFileInUse = 5,
        UnexpectedIlInstruction = 6,
    }
}
