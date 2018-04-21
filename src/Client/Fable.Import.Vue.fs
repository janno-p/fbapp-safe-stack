namespace Fable.Import

open Fable.Core

type Vue =
    [<Emit("new $0($1...)")>]
    abstract Create: obj -> Vue

[<Erase>]
module Vue =
    ()
