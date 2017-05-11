// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;

namespace BrainFckCompilerCS
{
    /// <summary>
    /// </summary>
    public class Instruction
    {
        /// <summary>
        /// The OpCode of this <see cref="Instruction"/>
        /// </summary>
        public OpCode OpCode { get; private set; }

        /// <summary>
        /// The Value of this <see cref="Instruction"/>
        /// </summary>
        public byte Value { get; set; }

        /// <summary>
        /// Creates a new instruction with <see cref="OpCode"/> = <paramref name="opCode"/> and <see
        /// cref="Value"/> = <paramref name="value"/>
        /// </summary>
        /// <param name="opCode">The <see cref="BrainFckCompilerCS.OpCode"/> of the new instruction</param>
        /// <param name="value">the value of the new instruction.</param>
        public Instruction(OpCode opCode, byte value)
        {
            this.OpCode = opCode;
            this.Value = value;
        }

        /// <summary>
        /// Creates a new instruction with <c><see cref="OpCode"/> = <paramref name="opCode"/></c>
        /// and <c><see cref="Value"/> = 1</c>
        /// </summary>
        /// <param name="opCode"></param>
        public Instruction(OpCode opCode)
        {
            this.OpCode = opCode;
            this.Value = 1;
        }

        /// <summary>
        /// Sets this instruction to a NoOp. Also sets the value to zero, even though that doesn't
        /// really matter.
        /// </summary>
        public void Invalidate()
        {
            this.OpCode = OpCode.NoOp;
            this.Value = 0;
        }

        /// <summary>
        /// Creates a new string that represents this <see cref="Instruction"/> in the format {OPCODE
        /// as hex}: {VALUE as hex}
        /// </summary>
        /// <returns><c>string.Format("{0:X}: {1:X2}", this.OpCode, this.Value)</c></returns>
        public override string ToString()
        {
            return string.Format("{0:X}: {1:X2}", this.OpCode, this.Value);
        }

        /// <summary>
        /// Creates a new string that represents this <see cref="Instruction"/>
        /// </summary>
        /// <param name="isCSharpString">Should this be outputted as CSharp code?</param>
        /// <returns>A string that represents this <see cref="Instruction"/></returns>
        public string ToString(bool isCSharpString)
        {
            if (!isCSharpString)
            {
                return ToString();
            }
            switch (this.OpCode)
            {
                case OpCode.NoOp:
                    return "";

                case OpCode.AddVal:
                    return $"ram[ptr]+={this.Value};";

                case OpCode.SubVal:
                    return $"ram[ptr]-={this.Value};";

                case OpCode.AddPtr:
                    return $"ptr+={this.Value};";

                case OpCode.SubPtr:
                    return $"ptr-={this.Value};";

                case OpCode.GetInput:
                    return "ram[ptr]=byte.Parse(Console.ReadLine());";

                case OpCode.SetOutput:
                    return "Console.WriteLine(ram[ptr] + \" \" + (char)ram[ptr]);";

                case OpCode.StartLoop:
                    return "while(ram[ptr]>0){";

                case OpCode.EndLoop:
                    return "}";

                case OpCode.AssignVal:
                    return $"ram[ptr]={this.Value};";
                default:
                    throw new InvalidOperationException("Unexpected OpCode" + this.OpCode);
            }
        }
    }
}
