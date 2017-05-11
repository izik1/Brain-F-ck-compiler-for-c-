// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
namespace BrainFckCompilerCS
{
    /// <summary>
    /// Represents the output of the compiler.
    /// </summary>
    public struct CompilerOutput
    {
        /// <summary>
        /// The success state of the compilation.
        /// </summary>
        public bool CompileSuccess { get; }

        /// <summary>
        /// The output of the compilation
        /// </summary>
        public string ErrorText { get; }

        public CompilerOutput(bool compileSuccess, string outputText)
        {
            this.CompileSuccess = compileSuccess;
            this.ErrorText = outputText;
        }
    }
}
