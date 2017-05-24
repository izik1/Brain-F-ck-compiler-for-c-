// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// The settings that determine how the compiler should behave. such settings include
    /// optimizations, file names, and input code.
    /// </summary>
    public class CompilerSettings
    {
        /// <summary>
        /// Should the compiler eliminate stores (assigns) that get written over right after?
        /// </summary>
        public bool EliminateDeadStores { get; set; }

        /// <summary>
        /// Should the compiler Eliminate empty loops? ([] is all that gets removed)
        /// </summary>
        public bool EliminateEmptyLoops { get; set; }

        /// <summary>
        /// Should the Compiler eliminate redundent code? (code that counters itself like +-)
        /// </summary>
        public bool EliminateRedundentCode { get; set; }

        public bool EliminateUnreachableLoops { get; set; }

        /// <summary>
        /// File name of the output CSharp source, if put as an empty string, no file will be
        /// outputted. DO NOT give a path and DO NOT give an extension (unless a second extension is good)
        /// </summary>
        public string FileNameCSharpSrc { get; set; } = "output-src";

        /// <summary>
        /// File name of the Generated IL code, if put as an empty string, no file will be outputted.
        /// DO NOT give a path and DO NOT give an extension (unless a second extension is good)
        /// </summary>
        public string FileNameIL { get; set; } = "IL";

        /// <summary>
        /// File name of the output CSharp source, if put as an empty string, the default string
        /// "output" will be used as the file name. DO NOT give a path and DO NOT give an extension
        /// (unless a second extension is good)
        /// </summary>
        public string FileNameOutputExe { get; set; } = "output";

        /// <summary>
        /// File name of the Users inputted code, if put as an empty string, no file will be
        /// outputted. DO NOT give a path and DO NOT give an extension (unless a second extension is good)
        /// </summary>
        public string FileNameUserCode { get; set; } = "input-code";

        /// <summary>
        /// Should <see cref="InputCode"/> be used as a path or code?
        /// </summary>
        public bool GetCodeFromFile { get; set; }

        /// <summary>
        /// The code/path to the code that the user gave.
        /// </summary>
        public string InputCode { get; set; }

        /// <summary>
        /// Should instructions that assign values be merged with instructions that modify values?
        /// (CSharp ex: x=2; x+=10; =&gt; x=12;)
        /// </summary>
        public bool MergeAssignThenModifyInstructions { get; set; }

        /// <summary>
        /// Should loops that assign the value at the current pointer to zero be simplified?
        /// </summary>
        public bool SimplifyAssignZeroLoops { get; set; } // Basically loops that set the value at the current pointer to 0.
    }
}
