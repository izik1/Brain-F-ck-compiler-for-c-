// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.Text;

public static class Program
{
    private static readonly string appdir = AppDomain.CurrentDomain.BaseDirectory;

    public static event Action OnCompile;

    public static event Action OnOpen;

    private static void Main()
    {
        UIHandler.SubscribeEvents();
        OnOpen();
        OnCompile();

        Console.WriteLine((Compile() ? "Compiled!" : "Failed to compile"));
    }

    // I have no clue what to name this
    public static string GetTransformedString(char prev, int runLength)
    {
        switch (prev)
        {
            case '>':
                return "ptr+=" + runLength + ';';

            case '<':
                return "ptr-=" + runLength + ';';

            case '+':
                return "ram[ptr]+=" + runLength + ';';

            case '-':
                return "ram[ptr]-=" + runLength + ';';

            case ',':
                return "ram[ptr]=byte.Parse(Console.ReadLine());";

            case '.':
                return "Console.WriteLine(ram[ptr] + \" \" + (char)ram[ptr]);";

            case '[':
                return "while(ram[ptr]>0){";

            case ']':
                return "}";

            default:
                Console.WriteLine("Invalid char slipped past parsing.");
                return "";
        }
    }

    private static string GetInjectString(string codeIn)
    {
        string code = codeIn + " ";
        StringBuilder inject = new StringBuilder();
        char prev = code[0];
        int runLength = 1; // The reason this is 1 instead of 0 is because runs are positive length.
        for (int i = 1; i < code.Length; i++)
        {
            if (prev == code[i] && ("+-><".IndexOf(code[i]) != -1))
            {
                runLength++;
            }
            else
            {
                inject.Append(GetTransformedString(prev, runLength));
                prev = code[i];

                runLength = 1;
            }
        }
        return inject.ToString();
    }

    private static bool Compile()
    {
        string code = Lexer.Lex(Settings.InputCode);
        code = Optimizer.Optimize(code);
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

        WriteToFiles(code, compiled, results);
        return true;
    }

    private static void WriteToFiles(string code, string compiled, CompilerResults results)
    {
        if (!Settings.GetCodeFromFile)
        {
            File.WriteAllText(appdir + "input-code.txt", Settings.InputCode);
        }
        File.WriteAllText(appdir + "optimized-code.txt", code);
        File.WriteAllBytes(appdir + "output.exe", File.ReadAllBytes(results.PathToAssembly));
        File.WriteAllText(appdir + "output-src.cs", compiled);
    }
}
