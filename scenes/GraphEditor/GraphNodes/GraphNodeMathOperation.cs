using Godot;
using System;
using FS;

public class GraphNodeMathOperation : FS.NodeGraph.Math.NodeGraphNodeMathOperator
{
    public void SetOperator(FS.Utils.MathOperator op)
    {
        this.Operator = op;
    }
}
