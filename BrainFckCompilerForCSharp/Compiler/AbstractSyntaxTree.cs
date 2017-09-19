using System.Collections;
using System.Collections.Generic;

namespace BrainFckCompilerCSharp
{
    internal sealed class AbstractSyntaxTree : IEnumerable<AbstractSyntaxTree>
    {
        internal OpCode Op;

        public AbstractSyntaxTree(OpCode opcode) => this.Op = opcode;

        public int Count => this.childNodes.Count;

        internal List<AbstractSyntaxTree> childNodes { get; private set; } = new List<AbstractSyntaxTree>();

        public AbstractSyntaxTree this[int index]
        {
            get => childNodes[index];
        }

        public void Add(AbstractSyntaxTree item) => this.childNodes.Add(item);

        public void Clear() => this.childNodes.Clear();

        public void ColapseNops()
        {
            foreach (AbstractSyntaxTree item in this.childNodes)
            {
                item.ColapseNops();
            }

            if (this.childNodes.Count == 1 && this.Op == OpCode.NoOp)
            {
                this.Op = this.childNodes[0].Op;

                // This batmans the child node and this node's childNode list, but not the child
                // node's subtree.
                this.childNodes = this.childNodes[0].childNodes;
            }
        }

        public IEnumerator<AbstractSyntaxTree> GetEnumerator()
        {
            foreach (AbstractSyntaxTree item in this.childNodes)
            {
                yield return item;
            }

            yield return this;
        }

        public List<Instruction> ToIl()
        {
            List<Instruction> il = new List<Instruction>();
            ToIl(il);
            return il;
        }

        internal void Remove(AbstractSyntaxTree ast) => this.childNodes.Remove(ast);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void ToIl(List<Instruction> il)
        {
            if (this.Op == OpCode.Loop)
            {
                il.Add(new Instruction(OpCode.StartLoop));
                foreach (AbstractSyntaxTree item in this.childNodes)
                {
                    item.ToIl(il);
                }

                il.Add(new Instruction(OpCode.EndLoop));
            }
            else
            {
                il.Add(new Instruction(this.Op));
                if (this.Op == OpCode.NoOp)
                {
                    foreach (AbstractSyntaxTree item in this.childNodes)
                    {
                        item.ToIl(il);
                    }
                }
            }
        }
    }
}
