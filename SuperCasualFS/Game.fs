namespace FS

open Godot


type Game() as _this =
    inherit Node()

    [<Export>]
    let startupText = "Hello World!"

    override this._Ready() = 
        GD.Print(startupText)


type GameButton() as _this =
    inherit Node()

    [<Export>]
    let buttonText = "I am a button"

    member this.Press() =
        GD.Print(buttonText);
