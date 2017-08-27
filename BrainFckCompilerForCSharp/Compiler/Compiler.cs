// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;

namespace BrainFckCompilerCSharp
{
    public static class Compiler
    {
        /// <summary>
        /// Attempts to compile the code specified in <paramref name="settings"/> outputs code in output.exe
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The success of the compilation.</returns>
        public static ErrorCodes Compile(CompilerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.InputCode == null)
            {
                throw new ArgumentException(nameof(settings.InputCode));
            }

            List<Instruction> IL = Lexer.Lex(settings.InputCode);
            ErrorCodes ValidationState = ProgramValidator.Validate(IL);
            if (ValidationState != ErrorCodes.Successful)
            {
                return ValidationState; // Invalid programs can't compile.
            }

            Optimizer.Optimize(IL, settings);
            ValidationState = ProgramValidator.Validate(IL);
            if (ValidationState != ErrorCodes.Successful)
            {
                return ValidationState; // Optimizations broke the code.
            }

            // Why is this using a constant string? because a better way to do this hasn't been
            // found. The reason why this isn't a $ string is because of the brackets.
            string compiled =
                "using System;public class Program{public static void Main(){byte[] ram=new byte[256];" +
                GetInjectString(IL) +
                "Console.ReadKey();}}";

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                CompilerParameters paramaters = new CompilerParameters
                {
                    CompilerOptions = "/optimize", // This currently does nothing?
                    GenerateExecutable = true,
                    OutputAssembly = Path.Combine(
                        appdir,
                        ((!string.IsNullOrEmpty(settings.FileNameOutputExe))
                        ? settings.FileNameOutputExe : "output") + ".exe")
                };

                if (provider.CompileAssemblyFromSource(paramaters, compiled).Errors.Count > 0)
                {
                    // This *shouldn't* ever happen because all errors *should* be caught by the
                    // validator. The only known instance of this occuring is when the application
                    // can't write to the file.
                    return ErrorCodes.UnknownCompilationFailure;
                }
            }

            // create a string which contains all the IL on new lines & pass the other args.
            WriteToFiles(string.Join(Environment.NewLine, IL), compiled, settings);
            return ErrorCodes.Successful; // Made it.
        }

        /// <summary>
        /// A string representing the directory the application was started in.
        /// </summary>
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Creates a string that is <paramref name="IL"/> as CSharp code.
        /// </summary>
        /// <param name="IL">The code to transpile to CSharp</param>
        /// <returns></returns>
        private static string GetInjectString(IList<Instruction> IL)
        {
            StringBuilder inject = new StringBuilder(10 * IL.Count);

            // these few lines are for checking to see if the first instruction would be to add to /
            // subtract from the pointer because if it is a little bit of micro optimization can be
            // made by just assigning it flat out.
            bool skipFirst = IL[0].OpCode == OpCode.AddPtr || IL[0].OpCode == OpCode.SubPtr;
            inject.Append((skipFirst) ? "byte ptr" +
                (((IL[0].OpCode == OpCode.AddPtr) ? IL[0].Value : (byte)(255 - IL[0].Value)).ToString()) + ";" : "byte ptr=0;");

            for (int i = (skipFirst) ? 1 : 0; i < IL.Count; i++)
            {
                inject.Append(IL[i].ToString(true));
            }

            return inject.ToString();
        }

        /// <summary>
        /// Writes the compiled exe along with the input code (brainf*ck), CSharp source and IL to files.
        /// </summary>
        /// <param name="IlString">The IL as a string. (gets written to IL.txt)</param>
        /// <param name="outputSrc">The CSharp source code of the output. (gets written to output-src.cs)</param>
        /// <param name="settings">The settings.</param>
        private static void WriteToFiles(string IlString, string outputSrc, CompilerSettings settings)
        {
            if (!string.IsNullOrEmpty(settings.FileNameUserCode))
            {
                File.WriteAllText(Path.Combine(appdir, settings.FileNameUserCode + ".txt"), settings.InputCode);
            }

            if (!string.IsNullOrEmpty(settings.FileNameIL))
            {
                File.WriteAllText(Path.Combine(appdir, settings.FileNameIL + ".txt"), IlString);
            }

            if (!string.IsNullOrEmpty(settings.FileNameCSharpSrc))
            {
                File.WriteAllText(Path.Combine(appdir, settings.FileNameCSharpSrc + ".cs"), outputSrc);
            }
        }
    }
}
