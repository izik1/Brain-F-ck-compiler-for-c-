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
                new Instruction(OpCode.GetInput),
                new Instruction(OpCode.SetOutput),
                new Instruction(OpCode.AssignVal),
                new Instruction(OpCode.AssignVal)
            };
            Optimizer.EliminateDeadStores(Instructions);
            Assert.IsTrue(Instructions.Count == 3);
            Assert.IsTrue(Instructions[0].OpCode == OpCode.GetInput && Instructions[1].OpCode == OpCode.SetOutput &&
                Instructions[2].OpCode == OpCode.AssignVal);
        }

        [TestMethod]
        public void EliminateEmptyLoopsTest()
        {
            List<Instruction> Instructions = new List<Instruction>
            {
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.EndLoop),
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.AddVal),
                new Instruction(OpCode.EndLoop)
            };
            Optimizer.EliminateEmptyLoops(Instructions);
            Assert.IsTrue(Instructions.Count == 3);
            Assert.IsTrue(Instructions[1].OpCode == OpCode.AddVal);
        }

        [TestMethod]
        public void EliminateRedundencyTest()
        {
            List<Instruction> Instructions = new List<Instruction>
            {
                new Instruction(OpCode.AddVal,2),
                new Instruction(OpCode.SubVal,3),
                new Instruction(OpCode.AddPtr,2),
                new Instruction(OpCode.SubPtr,2),
                new Instruction(OpCode.AddVal,3),
                new Instruction(OpCode.SubVal),
                new Instruction(OpCode.SubVal),
                new Instruction(OpCode.AddPtr,1),
                new Instruction(OpCode.GetInput,1),
                new Instruction(OpCode.SetOutput,1),
            };
            int len = 0;
            do
            {
                len = Instructions.Count;
                Optimizer.EliminateRedundency(Instructions);
            } while (len > Instructions.Count);
            Assert.IsTrue(Instructions.Count > 0);
            Assert.IsTrue(Instructions[0].OpCode == OpCode.AddPtr && Instructions[0].Value == 1);
        }

        [TestMethod]
        public void EliminateUnreachableLoopsTest()
        {
            List<Instruction> Instructions = new List<Instruction>
            {
                new Instruction(OpCode.AssignVal, 0),
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.AddPtr),
                new Instruction(OpCode.EndLoop),
            };
            Optimizer.EliminateUnreachableLoops(Instructions);
            Assert.IsTrue(Instructions.Count == 0);

            Instructions = new List<Instruction>
            {
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.AddPtr),
                new Instruction(OpCode.EndLoop),
            };
            Optimizer.EliminateUnreachableLoops(Instructions);
            Assert.IsFalse(Instructions.Count == 0);
        }

        [TestMethod]
        public void SimplifyAssignZeroLoopsTest()
        {
            List<Instruction> Instructions = new List<Instruction>
            {
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.AddVal),
                new Instruction(OpCode.EndLoop),
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.SubVal),
                new Instruction(OpCode.EndLoop),
                new Instruction(OpCode.StartLoop),
                new Instruction(OpCode.AddVal,2), // ensures that this loop can't be simplified
                new Instruction(OpCode.EndLoop),
            };
            Optimizer.SimplifyAssignZeroLoops(Instructions);
            Assert.IsTrue(Instructions.Count > 3);
            Assert.IsTrue(Instructions[0].OpCode == OpCode.AssignVal && Instructions[1].OpCode == OpCode.AssignVal);
            Assert.IsFalse(Instructions[2].OpCode == OpCode.AssignVal);
        }
    }
}
