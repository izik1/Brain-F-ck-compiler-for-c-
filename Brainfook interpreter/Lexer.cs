// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System.Text;

internal static class Lexer
{
    public static string Lex(string code)
    {
        return FilterComments(code);
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
