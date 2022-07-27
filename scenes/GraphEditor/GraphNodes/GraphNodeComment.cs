using Godot;
using System;
using FS;

[Tool]
public class GraphNodeComment : NodeGraph.Constant.NodeGraphNodeComment
{
    public string LabelReadyCB(RichTextLabel label)
    {
        this.SetLabel(label);
        return this.CommentText;
    }
}


