// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// An enumeration of opereration codes for the Internal IL used for this compiler.
    /// </summary>
    public enum OpCode
    {
        /// <summary>
        /// No Op stands for No Operation, no code is ran with these, but it's useful to mark
        /// instructions to be removed. Set to zero for semantics.
        /// </summary>
        NoOp = 0,

        /// <summary>
        /// Adds <see cref="Instruction.Value"/> to the value at the current pointer.
        /// </summary>
        AddVal,

        /// <summary>
        /// Subtracts <see cref="Instruction.Value"/> from the value at the current pointer.
        /// </summary>
        SubVal,

        /// <summary>
        /// Adds <see cref="Instruction.Value"/> to the current pointer.
        /// </summary>
        AddPtr,

        /// <summary>
        /// Subtracts <see cref="Instruction.Value"/> to the current pointer.
        /// </summary>
        SubPtr,

        /// <summary>
        /// Fetches one line of input from the standard input stream and sets the value at the
        /// current pointer equal to it.
        /// </summary>
        GetInput,

        /// <summary>
        /// Sends the value at the current pointer to the standard output stream.
        /// </summary>
        SetOutput,

        /// <summary>
        /// Begins a "while(value[ptr]&gt;0)" loop.
        /// </summary>
        StartLoop,

        /// <summary>
        /// Ends a loop.
        /// </summary>
        EndLoop,

        /// <summary>
        /// Assigns the value at the current pointer to <see cref="Instruction.Value"/>.
        /// </summary>
        AssignVal
    }
}
