module FS.Utils

open System


let toRads(degrees: float32) =
    ((float32) Math.PI / 180f) * degrees

let toDegrees(rads: float32) =
    (180f / (float32) Math.PI) * rads