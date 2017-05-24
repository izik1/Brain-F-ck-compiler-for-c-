// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;

namespace BrainFckCompilerCSharp
{
    internal static class Program
    {
        /// <summary>
        /// The Entry point of the application just does some simple UI requests and runs <see cref="Compiler.Compile(CompilerSettings)"/>
        /// </summary>
        private static void Main()
        {
            (bool Success, string ErrorText) output = Compiler.Compile(UIHandler.GetUserSettings());
            Console.WriteLine(output.Success ? "Compiled!" : output.ErrorText);
            Console.ReadKey(); // to prevent the application from closing.
        }
    }
}
