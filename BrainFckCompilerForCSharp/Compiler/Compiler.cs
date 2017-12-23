// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.Linq;

namespace BrainFckCompilerCSharp
{
    public static class Compiler
    {
        /// <summary>
        /// A string representing the directory the application was started in.
        /// </summary>
        private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;

        //CE ED 66 66 CC 0D 00 0B 03 73 00 83 00 0C 00 0D
        //00 08 11 1F 88 89 00 0E DC CC 6E E6 DD DD D9 99
        //BB BB 67 63 6E 0E EC CC DD DC 99 9F BB B9 33 3E
        private static readonly byte[] DmgHeader =
        {
            0x00,0xC3,0x50,0x01,0xCE,0xED,0x66,0x66,0xCC,0x0D,0x00,0x0B,0x03,0x73,0x00,0x83,
            0x00,0x0C,0x00,0x0D,0x00,0x08,0x11,0x1F,0x88,0x89,0x00,0x0E,0xDC,0xCC,0x6E,0xE6,
            0xDD,0xDD,0xD9,0x99,0xBB,0xBB,0x67,0x63,0x6E,0x0E,0xEC,0xCC,0xDD,0xDC,0x99,0x9F,
            0xBB,0xB9,0x33,0x3E,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,
            0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x00,0x0B,0x00,0x00,
        };

        /// <summary>
        /// Attempts to compile the code specified in <paramref name="settings"/> outputs code in output.exe
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>The success of the compilation.</returns>
        public static ErrorCodes Compile(CompilerSettings settings)
        {
            (ErrorCodes errorCode, List<Instruction> Il) preCompResults = CompileCommon(settings);
            if (preCompResults.errorCode != ErrorCodes.Successful)
            {
                return preCompResults.errorCode;
            }

            // Why is this using a constant string? because a better way to do this hasn't been
            // found. The reason why this isn't a $ string is because of the brackets.
            string compiled =
                "using System;public class Program{public static void Main(){byte[] ram=new byte[256];" +
                GetInjectString(preCompResults.Il) +
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
            WriteToFiles(string.Join(Environment.NewLine, preCompResults.Il), compiled, settings);
            return ErrorCodes.Successful; // Made it.
        }

        /// <summary>
        /// Converts given user code into a IL program which can then be compiled.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public static (ErrorCodes errorCode, List<Instruction> Il) CompileAst(CompilerSettings settings)
        {
            AbstractSyntaxTree tree = Lexer.LexAst(settings.InputCode);
            Optimizer.Optimize(tree, settings);
            List<Instruction> Il = tree.ToIl();
            Optimizer.Optimize(Il, settings);
            return (ErrorCodes.Successful, Il);
        }

        public static ErrorCodes CompileDmg(CompilerSettings settings)
        {
            (ErrorCodes errorCode, List<Instruction> Il) preCompResults = CompileCommon(settings);
            if (preCompResults.errorCode != ErrorCodes.Successful)
            {
                return preCompResults.errorCode;
            }

            Stack<int> jumpOffsets = new Stack<int>();
            List<byte> rom = new byte[0x100].Concat(DmgHeader).ToList();
            rom.Add(0xAF); // xor A
            rom.Add(0x47); // ld B,A (set B to 0)
            rom.Add(0x6F); // ld L,A (set L to 0)

            // ld H,C0
            rom.Add(0x26);
            rom.Add(0xC0);

            foreach (Instruction instr in preCompResults.Il)
            {
                int instrOffset = rom.Count;
                switch (instr.OpCode)
                {
                    case OpCode.Nop:
                        rom.Add(0x00); // Maybe emit a compiler warning here?
                        break;

                    case OpCode.AddVal:
                    case OpCode.SubVal:
                        rom.Add(instr.OpCode == OpCode.AddVal ? (byte)0xC6 : (byte)0xD6);
                        rom.Add(instr.Value);
                        break;

                    case OpCode.AddPtr:
                    case OpCode.SubPtr:
                        rom.Add(0x77); // Store A into the pointer.

                        // Load a with the address
                        rom.Add(0x3E);
                        rom.Add(instr.Value);
                        rom.Add(instr.OpCode == OpCode.AddPtr ? (byte)0x85 : (byte)0x95); // Modify the address.
                        rom.Add(0x6F); // Store the result into L
                        rom.Add(0x7E); // Read at the new pointer.
                        rom.Add(0xBB);
                        break;

                    case OpCode.StartLoop:
                        rom.Add(0xCA);
                        rom.Add(0);
                        rom.Add(0);
                        jumpOffsets.Push(instrOffset);
                        break;

                    case OpCode.EndLoop:
                        int baseAddr = jumpOffsets.Pop();

                        // Setup jump to the instruction after this.
                        rom[baseAddr + 1] = (byte)(instrOffset + 3);
                        rom[baseAddr + 2] = (byte)((instrOffset + 3) >> 8);

                        // Setup this jump.
                        rom.Add(0xC2);
                        rom.Add((byte)(baseAddr + 3));
                        rom.Add((byte)((baseAddr + 3) >> 8));
                        break;

                    case OpCode.AssignVal:
                        rom.Add(0x3E);
                        rom.Add(instr.Value);
                        break;

                    case OpCode.AssignZero:
                        rom.Add(0xAF);
                        break;

                    case OpCode.ScanLeft:
                    case OpCode.ScanRight:
                        rom.Add(0x28);
                        rom.Add(0x05); // this is a fixed address based on the start address of this.
                        rom.Add(0xAF);
                        rom.Add(instr.OpCode == OpCode.ScanRight ? (byte)0x2C : (byte)0x2D); // INC/DEC L
                        rom.Add(0xBE);
                        rom.Add(0x20); // JR NZ
                        rom.Add(0xFC); // addr: INC/DEC L.
                        break;

                    case OpCode.GetInput: // TODO: load from serial.
                    case OpCode.SetOutput: // TODO: write to serial.
                    default:
                        return ErrorCodes.UnexpectedIlInstruction;
                }
            }

            rom.Add(0x10);
            rom.Add(0x00);

            File.WriteAllBytes(((!string.IsNullOrEmpty(settings.FileNameOutputExe))
                        ? settings.FileNameOutputExe : "output") + ".gb", rom.ToArray());
            if (!string.IsNullOrEmpty(settings.FileNameIL))
            {
                File.WriteAllText(Path.Combine(appdir, settings.FileNameIL + ".txt"),
                    string.Join(Environment.NewLine, preCompResults.Il));
            }

            return ErrorCodes.Successful;
        }

        private static (ErrorCodes errorCode, List<Instruction> Il) CompileCommon(CompilerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (settings.InputCode == null)
            {
                throw new ArgumentException(nameof(settings.InputCode));
            }

            return CompileAst(settings);
        }

        /// <summary>
        /// Creates a string that is <paramref name="IL"/> as CSharp code.
        /// </summary>
        /// <param name="IL">The code to transpile to CSharp</param>
        /// <returns></returns>
        private static string GetInjectString(IList<Instruction> IL)
        {
            StringBuilder inject = new StringBuilder(10 * IL.Count);
            if (IL.Count == 0)
            {
                return "";
            }

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
