using Microsoft.VisualStudio.TestTools.UnitTesting;
using BrainFckCompilerCSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrainFckCompilerCSharp.Tests
{
    [TestClass()]
    public class OptimizerTests
    {
        [TestMethod()]
        public void EliminateDeadStoresTest()
        {
            List<Instruction> Instructions = new List<Instruction>{
                new Instruction(OpCode.GetInput),
                new Instruction(OpCode.AssignVal),
                new Instruction(OpCode.GetInput)
            };
            Optimizer.EliminateDeadStores(Instructions);
            Assert.IsTrue(Instructions.Count == 1);
            Assert.IsTrue(Instructions[0].OpCode == OpCode.GetInput);
        }
    }
}
