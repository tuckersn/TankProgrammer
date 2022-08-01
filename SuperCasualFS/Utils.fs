module FS.Utils

open System
open Godot

let toRads(degrees: float32) =
    ((float32) Math.PI / 180f) * degrees

let toDegrees(rads: float32) =
    (180f / (float32) Math.PI) * rads


//https://github.com/putridparrot/FSharp.Units/blob/master/FSharp.Units/Angle.fs
[<Measure>]
type rad =
    static member create(value: float32) = LanguagePrimitives.Float32WithMeasure<rad> value
    static member deg(rad: float32<rad>) =
        deg.create(toDegrees(float32 rad))
    static member (+) (r: float32<rad>, d: float32<deg>) =
        r + deg.rad(d)
and
    [<Measure>] deg =
    static member create(value: float32) = LanguagePrimitives.Float32WithMeasure<deg> value
    static member rad(deg: float32<deg>) =
        rad.create(toRads(float32 deg))
    static member (+) (r: float32<rad>, d: float32<deg>) =
        r + deg.rad(d)
       

type BooleanOperator =
    | GreaterThan = 0uy
    | LessThan = 1uy
    | Equals = 2uy
    | GreaterOrEquals = 3uy
    | LessOrEquals = 4uy
    | Not = 5uy

type MathOperator =
    | Add = 0uy
    | Subtract = 1uy
    | Multiply = 2uy
    | Divide = 3uy
    | Exponent = 4uy
    | Log = 5uy
    | Abs = 6uy
    | Negate = 7uy

//https://fsharp.github.io/fsharp-core-docs/reference/fsharp-core-operators.html
let LogBase(baseNumber: float32, value: float32) = (log value) / (log baseNumber)