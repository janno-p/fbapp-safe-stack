module FbApp.Components.Messages

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue

let private comp = createEmpty<ComponentOptions>
comp.render <- (fun h -> h1 [] [str "Messages"])

[<ExportDefault>]
let exports = comp
