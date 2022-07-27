using Godot;
using System;
using FS;

public class Player : FS.Player.Player
{
    [Export]
    public NodePath PlatformerPath = null;
    public override NodePath platformerPath => this.PlatformerPath;
}
