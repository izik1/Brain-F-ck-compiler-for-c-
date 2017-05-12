// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System.Collections.Generic;
using System.Text;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// This class is for the parsing of BrainF*ck code into the internal IL.
    /// </summary>
    internal static class Lexer
    {
        /// <summary>
        /// Parses <paramref name="code"/> into IL.
        /// </summary>
        /// <param name="code">The string that gets parsed to IL.</param>
        /// <returns>Code as an ordered list of <see cref="Instruction"/>.</returns>
        public static List<Instruction> Lex(string code)
        {
            string filteredCode = FilterComments(code);
            List<Instruction> IL = new List<Instruction>();
            char prev = filteredCode[0];
            byte count = 1;
            for (int i = 1; i < filteredCode.Length; i++)
            {
                // Some operations can be run length compressed (+-><) so this does that.
                if (prev == filteredCode[i] && ("+-><".IndexOf(filteredCode[i]) != -1))
                {
                    count++;
                }
                else
                {
                    IL.Add(prev.ToIL(count));
                    prev = filteredCode[i];

                    count = 1;
                }
            }
            IL.Add(prev.ToIL(count)); // The loop misses the last item so this is the cleanest way to get it.
            return IL;
        }

        /// <summary>
        /// Removes all characters from <paramref name="inputCode"/> except the following:
        /// +-[],.&lt;&gt;
        /// </summary>
        /// <param name="inputCode">The string to filter.</param>
        /// <returns><paramref name="inputCode"/> without comments.</returns>
        private static string FilterComments(string inputCode)
        {
            StringBuilder sb = new StringBuilder(inputCode.Length);
            foreach (char c in inputCode)
            {
                if ("[+<,.>-]".IndexOf(c) >= 0)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
