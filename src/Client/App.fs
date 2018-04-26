module FbApp.Client.App

open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.Vue

type AppData =
    abstract currentTime: System.DateTime with get, set

let comp = createEmpty<ComponentOptions>
comp.name <- "app"

comp.data <-
    (fun () ->
        let data = createEmpty<AppData>
        data.currentTime <- System.DateTime.Now
        box data)

comp.render <-
    (fun h ->
        let d = jsThis<AppData>
        h1 [] [
            str "Hello, World!"
            p [] [str (sprintf "It's time %A" d.currentTime)]
        ])
