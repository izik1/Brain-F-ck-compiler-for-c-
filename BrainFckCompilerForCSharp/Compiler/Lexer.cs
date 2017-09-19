// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BrainFckCompilerCSharp
{
    /// <summary>
    /// This class is for the parsing of BrainF*ck code into the internal IL.
    /// </summary>
    internal static class Lexer
    {
        public static AbstractSyntaxTree LexAst(string code)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            if (code.Length == 0)
            {
                throw new ArgumentException("code must have a non-zero length", nameof(code));
            }

            int index = 0;
            return GenerateTree(ref index, FilterComments(code), true, OpCode.StartLoop);
        }

        /// <summary>
        /// Removes all characters from <paramref name="inputCode"/> except the following:
        /// +-[],.&lt;&gt;
        /// </summary>
        /// <param name="inputCode">The string to filter.</param>
        /// <returns><paramref name="inputCode"/> without comments.</returns>
        private static string FilterComments(string inputCode) =>
            Regex.Replace(inputCode, @"[^\[+<,.>\-\]]", "", RegexOptions.Compiled);

        private static AbstractSyntaxTree GenerateTree(ref int index, string code, bool isRoot, OpCode bundleType)
        {
            AbstractSyntaxTree tree = new AbstractSyntaxTree(OpCode.Loop);
            while (index < code.Length)
            {
                switch (code[index++])
                {
                    case '[':
                        tree.Add(GenerateTree(ref index, code, false, OpCode.StartLoop));
                        break;

                    case ']':
                        if (isRoot)
                        {
                            throw new InvalidOperationException(); // Unbalanced ']'
                        }

                        if (bundleType != OpCode.StartLoop)
                        {
                            index--;
                        }

                        return tree;

                    case '+':
                        tree.Add(new AbstractSyntaxTree(OpCode.AddVal));
                        break;

                    case '-':
                        tree.Add(new AbstractSyntaxTree(OpCode.SubVal));
                        break;

                    case '>':
                        tree.Add(new AbstractSyntaxTree(OpCode.AddPtr));
                        break;

                    case '<':
                        tree.Add(new AbstractSyntaxTree(OpCode.SubPtr));
                        break;

                    case '.':
                        tree.Add(new AbstractSyntaxTree(OpCode.SetOutput));
                        break;

                    case ',':
                        tree.Add(new AbstractSyntaxTree(OpCode.GetInput));
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            if (!isRoot)
            {
                throw new InvalidOperationException(); // Unbalanced '['
            }

            tree.Op = OpCode.Nop;

            return tree;
        }
    }
}
