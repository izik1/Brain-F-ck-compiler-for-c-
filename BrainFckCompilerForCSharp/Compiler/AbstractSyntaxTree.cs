using System.Collections;
using System.Collections.Generic;

namespace BrainFckCompilerCSharp
{
    public sealed class AbstractSyntaxTree : IEnumerable<AbstractSyntaxTree>
    {
        public OpCode Op { get; set; }

        public AbstractSyntaxTree(OpCode opcode) => this.Op = opcode;

        public int Count => this.ChildNodes.Count;

        public List<AbstractSyntaxTree> ChildNodes { get; private set; } = new List<AbstractSyntaxTree>();

        public AbstractSyntaxTree this[int index]
        {
            get => ChildNodes[index];
        }

        public void Add(AbstractSyntaxTree item) => this.ChildNodes.Add(item);

        public void Clear() => this.ChildNodes.Clear();

        public void ColapseNops() => ColapseNops(true);

        private void ColapseNops(bool trueRoot)
        {
            foreach (AbstractSyntaxTree item in this.ChildNodes)
            {
                item.ColapseNops(false);
            }

            if (!trueRoot && this.ChildNodes.Count == 1 && this.Op == OpCode.Nop)
            {
                this.Op = this.ChildNodes[0].Op;

                // This batmans the child node and this node's childNode list, but not the child
                // node's subtree.
                this.ChildNodes = this.ChildNodes[0].ChildNodes;
            }
        }

        public IEnumerator<AbstractSyntaxTree> GetEnumerator()
        {
            foreach (AbstractSyntaxTree item in this.ChildNodes)
            {
                yield return item;
            }

            yield return this;
        }

        public bool AllChildrenAssignZero => this.ChildNodes.TrueForAll(ast => ast.Op == OpCode.AssignZero);

        public bool AllChildrenIncVal => this.ChildNodes.TrueForAll(ast => ast.Op == OpCode.AddVal);

        public bool AllChildrenDecVal => this.ChildNodes.TrueForAll(ast => ast.Op == OpCode.SubVal);

        public void RemoveAt(int index) => this.ChildNodes.RemoveAt(index);

        public List<Instruction> ToIl()
        {
            List<Instruction> il = new List<Instruction>(this.ChildNodes.Count);
            ToIl(il);
            return il;
        }

        public void Remove(AbstractSyntaxTree ast) => this.ChildNodes.Remove(ast);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void ToIl(List<Instruction> il)
        {
            if (this.Op == OpCode.Loop)
            {
                il.Add(new Instruction(OpCode.StartLoop));
                foreach (AbstractSyntaxTree item in this.ChildNodes)
                {
                    item.ToIl(il);
                }

                il.Add(new Instruction(OpCode.EndLoop));
            }
            else
            {
                il.Add(new Instruction(this.Op));
                if (this.Op == OpCode.Nop)
                {
                    foreach (AbstractSyntaxTree item in this.ChildNodes)
                    {
                        item.ToIl(il);
                    }
                }
            }
        }
    }
}
