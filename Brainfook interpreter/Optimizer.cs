// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.
using System.Text;

public static class Optimizer
{
    public static string Optimize(string codeIn)
    {
        string code = codeIn;
        if (Settings.SimplifyToZeroLoops)
        {
            code = SimplifyToZeroLoops(code);
        }
        int len = 0;
        do
        {
            len = code.Length;
            if (Settings.EliminateRedundentCode)
            {
                code = EliminateRedundency(code);
            }
            if (Settings.EliminateRepeatedFlatValues)
            {
                code = EliminateRepeatedFlatValue(code);
            }
            code = code.TrimStart('0');
        } while (code.Length < len);

        return code;
    }

    private static char prev = ' ';

    private static string EliminateRedundency(string CodeIn)
    {
        if (CodeIn.Length == 0)
        {
            return "";
        }
        prev = CodeIn[0];
        int runLength = 1;
        StringBuilder output = new StringBuilder();
        string code = CodeIn + " ";
        for (int i = 1; i < code.Length; i++)
        {
            if (prev == code[i] && ("+-><".IndexOf(prev) != -1))
            {
                runLength++;
            }
            else
            {
                switch (prev)
                {
                    case '<':
                        runLength = GetRunString('<', '>', code[i], runLength, output);
                        break;

                    case '>':
                        runLength = GetRunString('>', '<', code[i], runLength, output);
                        break;

                    case '+':
                        runLength = GetRunString('+', '-', code[i], runLength, output);
                        break;

                    case '-':
                        runLength = GetRunString('-', '+', code[i], runLength, output);
                        break;

                    case ' ':
                        prev = code[i];
                        break;

                    default:
                        output.Append(code[i - 1]);
                        prev = code[i];
                        break;
                }
            }
        }
        return output.ToString();
    }

    private static string SimplifyToZeroLoops(string code)
    {
        return code.Replace("[+]", "0").Replace("[-]", "0");
    }

    private static string EliminateRepeatedFlatValue(string code)
    {
        if (code.Length == 0)
        {
            return "";
        }
        StringBuilder output = new StringBuilder();
        for (int i = 0; i < code.Length - 1; i++)
        {
            if ("0,".IndexOf(code[i + 1]) == -1 || "+-0,".IndexOf(code[i]) == -1)
            {
                output.Append(code[i]);
            }
        }
        output.Append(code[code.Length - 1]);
        return output.ToString();
    }

    private static int GetRunString(char normal, char inverse, char active, int runLength, StringBuilder output)
    {
        int lOut = runLength;
        if (active == inverse)
        {
            lOut--;
        }
        else
        {
            if (runLength > 0)
            {
                output.Append(normal, lOut);
            }
            else
            {
                output.Append(inverse, -lOut);
            }
            lOut = 1;
            prev = active;
        }

        return lOut;
    }
}
