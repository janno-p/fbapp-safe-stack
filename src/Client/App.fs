module FbApp.Client.App

open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue

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
        div [Class "app"] [
            com FbApp.Components.Navigation.comp

            div [Class "container app-content"] [
                routerView ()
            ]

            footer [Class "container"] [
                hr []
                p [] [str <| sprintf "FbApp, %d" d.currentTime.Year]
            ]
        ])
