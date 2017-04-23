using System.Text;

public static class RedundencyEliminator
{
    private static char prev = ' ';

    public static string Eliminate(string CodeIn)
    {
        int runLength = 1;
        StringBuilder output = new StringBuilder();
        string code = CodeIn + " ";
        for (int i = 0; i < code.Length; i++)
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
                        runLength = SubFunction('<', '>', code[i], runLength, output);
                        break;

                    case '>':
                        runLength = SubFunction('>', '<', code[i], runLength, output);
                        break;

                    case '+':
                        runLength = SubFunction('-', '+', code[i], runLength, output);
                        break;

                    case '-':
                        runLength = SubFunction('+', '-', code[i], runLength, output);
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

    private static int SubFunction(char normal, char inverse, char active, int runLength, StringBuilder output)
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
                output.Append(normal, runLength);
            }
            else
            {
                output.Append(inverse, -runLength);
            }
            lOut = 1;
            prev = active;
        }

        return lOut;
    }
}
