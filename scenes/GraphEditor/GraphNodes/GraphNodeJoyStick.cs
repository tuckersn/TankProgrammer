using Godot;
using System;
using FS;

public class GraphNodeJoyStick : FS.NodeGraph.NodeGraphNodeJoyStick
{

    [Export]
    public Vector2 defaultSize = new Vector2(160, 200);
    public void SetValue(Vector2 vec)
    {
        this.Vector = vec;
    }

    public void EmitResize()
    {
        this.EmitSignal("resize_request", defaultSize);
    }
}
