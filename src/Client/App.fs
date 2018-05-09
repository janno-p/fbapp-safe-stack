module FbApp.Client.App

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue

type AppData =
    abstract currentTime: System.DateTime with get, set

let private comp = createEmpty<ComponentOptions>
comp.name <- "app"

comp.data <-
    (fun () ->
        let data = createEmpty<AppData>
        data.currentTime <- System.DateTime.Now
        box data)

comp.render <-
    (fun h ->
        let d = jsThis<AppData>
        div [Attrs !!["id" ==> "app"]] [
            com FbApp.Client.Components.Navigation.comp

            div [Class "container app-content"] [
                routerView ()
            ]

            footer [Class "container"] [
                hr []
                p [] [str <| sprintf "FbApp, %d" d.currentTime.Year]
            ]
        ])

[<ExportDefault>]
let exports = comp
