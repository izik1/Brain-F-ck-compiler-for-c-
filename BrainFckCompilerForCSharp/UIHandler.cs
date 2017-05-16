// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.IO;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// A front end for the compiler.
    /// </summary>
    internal static class UIHandler
    {
        /// <summary>
        /// Asks the user for the compiler options.
        /// </summary>
        /// <returns>The users code/optmization choices.</returns>
        internal static CompilerSettings GetUserSettings()
        {
            bool getCodeFromFile = GetUserInput("Input from file?");

            return new CompilerSettings
            {
                EliminateRedundentCode = GetUserInput("Eliminate redundencies? (ex: ++- turns into + or ><< turns into <)"),
                EliminateDeadStores = GetUserInput(
                "Eliminate Repeated Dead Stores? " +
                "(storing a value and then storing another value on top without ever using the first one)"),
                SimplifyAssignZeroLoops = GetUserInput("Simplify Loops that assign 0?"),
                MergeAssignThenModifyInstructions = GetUserInput(
                "Merge assignment instructions with following add/sub instructions?"),
                EliminateEmptyLoops = GetUserInput("Eliminate Empty loops?"),
                GetCodeFromFile = getCodeFromFile,
                EliminateUnreachableLoops = GetUserInput("Eliminate unreachable loops?"),
                InputCode = GetUserCode(getCodeFromFile)
            };
        }

        /// <summary>
        /// Requsts a Yes / no input from the user.
        /// </summary>
        /// <returns>The users choice.</returns>
        internal static bool GetYnInput()
        {
            char c = Console.ReadKey(true).KeyChar;
            while ("YyNn".IndexOf(c) == -1)
            {
                Console.Clear();
                Console.WriteLine("Invalid input please type Y, y, N, or n");
                c = Console.ReadKey(true).KeyChar;
            }
            return (c == 'Y' || c == 'y');
        }

        /// <summary>
        /// An extension of <see cref="GetYnInput"/> clears the console, displays some text then
        /// calls that.
        /// </summary>
        /// <param name="displayText">The text to display to the user.</param>
        /// <returns>The users choice.</returns>
        internal static bool GetUserInput(string displayText)
        {
            Console.Clear();
            Console.WriteLine(displayText + " y/n?");
            return GetYnInput();
        }

        /// <summary>
        /// Gets the code or the path to the code that the user is using.
        /// </summary>
        /// <param name="FileIO">Is this a file path request or a string input request?</param>
        /// <returns>The users code in the form of a file path or string.</returns>
        private static string GetUserCode(bool FileIO)
        {
            Console.Clear();
            if (FileIO)
            {
                Console.WriteLine("Put code path here (note: must be a full path to an exact file)");
                string path = Console.ReadLine();
                while (!File.Exists(path))
                {
                    Console.Clear();
                    Console.WriteLine("Invalid path");
                    path = Console.ReadLine();
                }
                Console.Clear();
                return File.ReadAllText(path);
            }
            else
            {
                Console.WriteLine("Put code here, note that it must all be on 1 line.");
                return Console.ReadLine();
            }
        }
    }
}
