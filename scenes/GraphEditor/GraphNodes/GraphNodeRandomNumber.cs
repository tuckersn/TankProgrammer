using Godot;
using System;
using FS;

public class GraphNodeRandomNumber : FS.NodeGraph.Math.NodeGraphNodeRandomNumber
{

    public void SetMin(float number)
    {
        this.Min = number;
    }

    public void SetMax(float number)
    {
        this.Max = number;
    }

    public void SetCycles(float cycles)
    {
        this.Cycles = Convert.ToUInt32(cycles);
    }
}
