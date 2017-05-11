using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;

public static class Compiler
{
    private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;

    public static string ConvertIlInstructionToString(Instruction instruction)
    {
        switch (instruction.OpCode)
        {
            case OpCode.NoOp:
                return "";

            case OpCode.AddVal:
                return $"ram[ptr]+={instruction.Value};";

            case OpCode.SubVal:
                return $"ram[ptr]-={instruction.Value};";

            case OpCode.AddPtr:
                return $"ptr+={instruction.Value};";

            case OpCode.SubPtr:
                return $"ptr-={instruction.Value};";

            case OpCode.GetInput:
                return "ram[ptr]=byte.Parse(Console.ReadLine());";

            case OpCode.SetOutput:
                return "Console.WriteLine(ram[ptr] + \" \" + (char)ram[ptr]);";

            case OpCode.StartLoop:
                return "while(ram[ptr]>0){";

            case OpCode.EndLoop:
                return "}";

            case OpCode.AssignVal:
                return $"ram[ptr]={instruction.Value};";
            default:
                throw new InvalidOperationException("Unexpected OpCode" + instruction.OpCode);
        }
    }

    private static string GetInjectString(List<Instruction> code)
    {
        StringBuilder inject = new StringBuilder();
        for (int i = 0; i < code.Count; i++)
        {
            inject.Append(ConvertIlInstructionToString(code[i]));
        }
        return inject.ToString();
    }

    public static bool Compile(string codeIn)
    {
        List<Instruction> code = Lexer.Lex(codeIn);
        Optimizer.Optimize(code);
        if (!ProgramValidator.Validate(code))
        {
            return false;
        }
        string compiled = "using System;public class Program{public static void Main(){byte[] ram=new byte[256];byte ptr=0;" +
            GetInjectString(code) +
            "Console.ReadKey();}}";
        CSharpCodeProvider provider = new CSharpCodeProvider();
        CompilerParameters paramaters = new CompilerParameters
        {
            CompilerOptions = "/optimize",
            GenerateExecutable = true
        };

        CompilerResults results = provider.CompileAssemblyFromSource(paramaters, compiled);

        if (results.Errors.Count > 0)
        {
            return false;
        }

        WriteToFiles(string.Join("\n", code), compiled, results);
        return true;
    }

    private static void WriteToFiles(string code, string compiled, CompilerResults results)
    {
        if (!Settings.GetCodeFromFile)
        {
            File.WriteAllText(appdir + "input-code.txt", Settings.InputCode);
        }
        File.WriteAllText(appdir + "IL.txt", code);
        File.WriteAllBytes(appdir + "output.exe", File.ReadAllBytes(results.PathToAssembly));
        File.WriteAllText(appdir + "output-src.cs", compiled);
    }
}
