// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System.Text;

public static class RepeatedInputEliminator
{
    public static string Eliminate(string code)
    {
        StringBuilder sb = new StringBuilder(code.Length);
        char prev = ' ';
        for (int i = 0; i < code.Length; i++)
        {
            char current = code[i];
            if (prev != ',' || current != ',')
            {
                sb.Append(current);
                prev = current;
            }
        }
        return sb.ToString();
    }
}
