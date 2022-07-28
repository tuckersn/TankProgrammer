namespace FS.Player

open Godot
open System
open FS.Utils

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

type PlayerGun() as _this =
    inherit Node2D()

    [<Export>]
    let mutable rotation = 0f<rad>; 

    override this._Process(delta) =
        this.GlobalRotation <- float32 rotation

    override this._PhysicsProcess(delta) =
        rotation <- rotation + deg.rad(1f<deg>);

type PlayerBody() as _this =
    inherit KinematicBody2D()

    [<Export>]
    let mutable speed: float32 = 600f;
    [<Export>]
    let mutable rotationSpeed: float32 = 100f;
    [<Export>]
    let mutable animationSpeed: float32 = 10f;

    let mutable velocity = Vector2(0f, 10000f);

    let camera = lazy(_this.GetNode<Camera2D>(new NodePath("Camera2D")));
    let sprite = lazy(_this.GetNode<AnimatedSprite>(new NodePath("BodySprite")));

    override this._PhysicsProcess(delta) =

        match Player.Get() with
            | Some(player) ->
                camera.Value.Zoom <- Vector2.One * (2f - Platformer.ZoomLevel());
                let (movePercent, turnPercent, name) = Player.GetValues();
                this.Rotate(FS.Utils.toRads((rotationSpeed * (turnPercent * (1f - Math.Abs(movePercent) * 0.75f)))) * delta);
                let inputVelocity = Vector2(0f, speed * movePercent * delta).Rotated(this.Rotation);
                velocity <- this.MoveAndSlide(inputVelocity, Vector2.Up)

                let mutable animationDirection = "MovingForward"
                            
                if Math.Round(float(Math.Abs(sprite.Value.SpeedScale - animationSpeed * movePercent)), 1) > float(0) then
                    GD.Print(sprite.Value.SpeedScale, " === ", animationSpeed * movePercent, " DIFF = ", sprite.Value.SpeedScale - animationSpeed * movePercent);
                    let newSpeed = animationSpeed * movePercent

                    if newSpeed < 0f then
                        sprite.Value.SpeedScale <- animationSpeed * movePercent * -1f
                        animationDirection <- "MovingBackward"
                    else
                        sprite.Value.SpeedScale <- animationSpeed * movePercent

                if Math.Abs(movePercent) > 0f then
                    if not(sprite.Value.Animation = animationDirection) then
                        sprite.Value.Play(animationDirection)
                else
                    if not(sprite.Value.Animation = "default") then
                        sprite.Value.Play("default")
                    
            | None -> ()
                

        ()
