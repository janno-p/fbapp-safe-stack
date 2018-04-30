module FbApp.Client.Pages.Dashboard

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue

let private comp = createEmpty<ComponentOptions>
comp.render <- (fun h -> h1 [] [str "Dashboard"])

[<ExportDefault>]
let exports = comp
