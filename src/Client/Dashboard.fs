module FbApp.Components.Dashboard

open Fable.Core
open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.Vue

let private comp = createEmpty<ComponentOptions>
comp.render <- (fun h -> h1 [] [str "Dashboard"])

[<ExportDefault>]
let exports = comp
