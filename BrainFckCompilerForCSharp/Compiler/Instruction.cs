// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

using System;

namespace BrainFckCompilerCSharp
{
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
        /// Initializes a new instance of the <see cref="Instruction"/> class.
        /// </summary>
        /// <param name="opCode">The op code.</param>
        /// <param name="value">The value.</param>
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
        /// Sets this instruction to a Nop. Also sets the value to zero, even though that doesn't
        /// really matter.
        /// </summary>
        public void Invalidate()
        {
            this.OpCode = OpCode.Nop;
            this.Value = 0;
        }

        /// <summary>
        /// Creates a new string that represents this <see cref="Instruction"/> in the format {OPCODE
        /// as hex}: {VALUE as hex}
        /// </summary>
        /// <returns><c>$"{this.OpCode:X}: {this.Value:X2}"</c></returns>
        public override string ToString() => $"{this.OpCode:X}: {this.Value:X2}";

        /// <summary>
        /// Creates a new string that represents this <see cref="Instruction"/>
        /// </summary>
        /// <param name="isCSharpString">Should this be outputted as CSharp code?</param>
        /// <returns>A string that represents this <see cref="Instruction"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string ToString(bool isCSharpString)
        {
            if (!isCSharpString)
            {
                return ToString();
            }

            switch (this.OpCode)
            {
                case OpCode.Nop: return "";
                case OpCode.AddVal: return "ram[ptr]+=" + this.Value.ToString() + ";";
                case OpCode.SubVal: return "ram[ptr]-=" + this.Value.ToString() + ";";
                case OpCode.AddPtr: return "ptr+=" + this.Value.ToString() + ";";
                case OpCode.SubPtr: return "ptr-=" + this.Value.ToString() + ";";
                case OpCode.GetInput: return "ram[ptr]=byte.Parse(Console.ReadLine());";
                case OpCode.SetOutput: return "Console.WriteLine(ram[ptr] + \" \" + (char)ram[ptr]);";
                case OpCode.StartLoop: return "while(ram[ptr]>0){";
                case OpCode.EndLoop: return "}";
                case OpCode.ScanLeft: return "while(ram[ptr--]>0){}";
                case OpCode.ScanRight: return "while(ram[ptr++]>0){}";
                case OpCode.AssignVal: return "ram[ptr]=" + this.Value.ToString() + ";";
                case OpCode.AssignZero: return "ram[ptr]=0;";
                default: throw new InvalidOperationException("Unexpected OpCode" + this.OpCode.ToString());
            }
        }
    }
}
