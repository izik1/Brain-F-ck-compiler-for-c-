// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System.Collections.Generic;
using System.Text;

internal static class Lexer
{
    public static List<Instruction> Lex(string code)
    {
        string filteredCode = FilterComments(code);
        List<Instruction> IL = new List<Instruction>();
        char prev = filteredCode[0];
        byte count = 1;
        for (int i = 1; i < filteredCode.Length; i++)
        {
            if (prev == filteredCode[i] && ("+-><".IndexOf(filteredCode[i]) != -1))
            {
                count++;
            }
            else
            {
                IL.Add(ConvertToIL(prev, count));
                prev = filteredCode[i];

                count = 1;
            }
        }
        IL.Add(ConvertToIL(prev, count));
        return IL;
    }

    private static Instruction ConvertToIL(char c, byte count)
    {
        switch (c)
        {
            case '+':
                return new Instruction(OpCode.AddVal, count);

            case '-':
                return new Instruction(OpCode.SubVal, count);

            case '>':
                return new Instruction(OpCode.AddPtr, count);

            case '<':
                return new Instruction(OpCode.SubPtr, count);

            case ',':
                return new Instruction(OpCode.GetInput, 0);

            case '.':
                return new Instruction(OpCode.SetOutput, 0);

            case '[':
                return new Instruction(OpCode.StartLoop, 0);

            case ']':
                return new Instruction(OpCode.EndLoop, 0);

            default:
                throw new System.InvalidOperationException("Nothing should ever get here.");
        }
    }

    private static string FilterComments(string code)
    {
        StringBuilder sb = new StringBuilder(code.Length);
        foreach (char c in code)
        {
            if ("[+<,.>-]".IndexOf(c) >= 0)
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
