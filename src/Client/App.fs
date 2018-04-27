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
        div [Class "app"] [
            nav [Class "navbar navbar-expand-lg navbar-dark bg-dark fixed-top"] [
                div [Class "container"] [
                    routerLink [Class "navbar-brand"; Attrs !!["to" ==> "/"]] [
                        str "Ennustusmäng"
                    ]
                    button [Class "navbar-toggler"; Attrs !!["type" ==> "button"; "data-toggle" ==> "collapse"; "data-target" ==> "#navbarSupportedContent"; "aria-controls" ==> "navbarSupportedContent"; "aria-expanded" ==> "false"; "aria-label" ==> "Toggle navigation"]] [
                        span [Class "navbar-toggler-icon"] []
                    ]
                    div [Class "collapse navbar-collapse"; Attrs !!["id" ==> "navbarSupportedContent"]] [
                        ul [Class "navbar-nav mr-auto"] [
                            li [Class "nav-item"] [
                                routerLink [Class "nav-link"; Attrs !!["to" ==> "/"]] [str "Home"]
                            ]
                            li [Class "nav-item"] [
                                routerLink [Class "nav-link"; Attrs !!["to" ==> "/about"]] [str "About"]
                            ]
                            li [Class "nav-item"] [
                                routerLink [Class "nav-link disabled"; Attrs !!["to" ==> "/contact"]] [str "Contact"]
                            ]
                        ]
                        ul [Class "navbar-nav ml-auto"] [
                            li [Class "nav-item"] [
                                routerLink [Class "nav-link"; Attrs !!["to" ==> "/dashboard"]] [
                                    i [Class "fas fa-cog"] []
                                ]
                            ]
                        ]
                    ]
                ]
            ]
            div [Class "container app-content"] [
                routerView ()
            ]
            footer [Class "container"] [
                p [] [str (sprintf "© %d - FbApp" d.currentTime.Year)]
            ]
        ])
