// Copyright 2017 Zachery Gyurkovitz See LICENCE.md for the full licence.

public class Instruction
{
    public OpCode OpCode { get; private set; }

    public byte Value { get; set; }

    public Instruction(OpCode opCode, byte value)
    {
        this.OpCode = opCode;
        this.Value = value;
    }

    public Instruction(OpCode opCode)
    {
        this.OpCode = opCode;
        this.Value = 1;
    }

    /// <summary>
    /// Sets this instruction to a NoOp. Also sets the value to zero, even though it doesn't really matter.
    /// </summary>
    public void Invalidate()
    {
        this.OpCode = OpCode.NoOp;
        this.Value = 0;
    }

    public override string ToString()
    {
        return string.Format("{0:X}: {1:X2}", this.OpCode, this.Value);
    }
}
