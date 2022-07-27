namespace FS.Player

open Godot
open System


type Platformer() as _this =
    inherit Node2D()

    static let mutable zoomLevel = 1f;

    member this.ZoomChange(level: float32) =
        zoomLevel <- level

    static member ZoomLevel() = zoomLevel


[<AbstractClass>]
type Player() as _this =
    inherit Node2D()

    static let mutable movementSpeed = 0f;
    static let mutable turnSpeed = 0f;
    static let mutable name = "";

    static let mutable self: Option<Player> = None;

    static member MoveSpeed() = movementSpeed;
    static member TurnSpeed() = turnSpeed;
    static member Name() = name;
    static member GetValues() = (movementSpeed, turnSpeed, name);

    static member Update(ms: float32, ts: float32, n: string) =
        movementSpeed <- ms
        turnSpeed <- ts
        name <- n

    static member Get() =
        self

    override this._Ready() =
        self <- Some(this)

    abstract platformerPath: NodePath;
    

type PlayerRayCaster() as _this =
    inherit RayCast2D()


type PlayerBody() as _this =
    inherit KinematicBody2D()

    [<Export>]
    let mutable speed: float32 = 600f;
    [<Export>]
    let mutable rotationSpeed: float32 = 20f;

    let velocity = Vector2(0f, 10000f);

    let camera = lazy(_this.GetNode<Camera2D>(new NodePath("Camera2D")));

    override this._PhysicsProcess(delta) =

        match Player.Get() with
            | Some(player) ->
                camera.Value.Zoom <- Vector2.One * (2f - Platformer.ZoomLevel());

                let (movePercent, turnPercent, name) = Player.GetValues();
                this.Rotate(FS.Utils.toRads(rotationSpeed * turnPercent * delta));
                let inputVelocity = Vector2(0f, speed * movePercent * delta).Rotated(this.Rotation);
                velocity = this.MoveAndSlide(inputVelocity, Vector2.Up) |> ignore
            | None -> ()
                

        ()
