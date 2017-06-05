using System;
using BrainFckCompilerCSharp;
using System.IO;

namespace BrainFckCompilerForCSharpCommandLine
{
    internal static class Program
    {
        private static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine($"{Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0])} [<switches>...] <path to source>");
            Console.WriteLine("<Switches>");
            Console.WriteLine("-d : Output debug files (IL and C#)");
            Console.WriteLine("-o : Enable all optimizations");
            Console.WriteLine("-ds : Enable the EliminateDeadStore optimization");
            Console.WriteLine("-rc : Enable the EliminateRedundentCode optimization");
            Console.WriteLine("-ul : Enable the EliminateUnreachableLoops optimization");
            Console.WriteLine("-matmi : Enable the MergeAssignThenModifyInstructions optimization");
            Console.WriteLine("-sazl : Enable the SimplifyAssignZeroLoops optimization");
        }

        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }
            CompilerSettings settings = new CompilerSettings()
            {
                FileNameUserCode = string.Empty,
                FileNameIL = string.Empty,
                FileNameCSharpSrc = string.Empty,
                EliminateEmptyLoops = false
            };
            for (int i = 0; i < args.Length - 1; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "-d":
                        settings.FileNameIL = "IL";
                        settings.FileNameCSharpSrc = "output-src";
                        break;

                    case "-o":
                        settings.EliminateDeadStores = true;
                        settings.EliminateRedundentCode = true;
                        settings.EliminateUnreachableLoops = true;
                        settings.MergeAssignThenModifyInstructions = true;
                        settings.SimplifyAssignZeroLoops = true;
                        break;

                    case "-ds":
                        settings.EliminateDeadStores = true;
                        break;

                    case "-rc":
                        settings.EliminateRedundentCode = true;
                        break;

                    case "-ul":
                        settings.EliminateUnreachableLoops = true;
                        break;

                    case "-matmi":
                        settings.MergeAssignThenModifyInstructions = true;
                        break;

                    case "-sazl":
                        settings.SimplifyAssignZeroLoops = true;
                        break;

                    default:
                        Console.WriteLine($"Invalid argument \"{args[i]}\"");
                        return 2;
                }
            }
            if (!System.IO.File.Exists(args[args.Length - 1]))
            {
                Console.WriteLine($"{args[args.Length - 1]} is not a valid file path");
                return 3;
            }
            settings.InputCode = System.IO.File.ReadAllText(args[args.Length - 1]);
            (bool Success, string ErrorText) output = Compiler.Compile(settings);
            Console.WriteLine(output.Success ? "Compiled." : output.ErrorText);
            return 0;
        }
    }
}
