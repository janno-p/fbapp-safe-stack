module FbApp.Client.Dashboard

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue

let private comp = createEmpty<ComponentOptions>
comp.name <- "dashboard"

comp.render <-
    (fun h ->
        div [Attrs !!["id" ==> "app"]; Class "dashboard"] [
            nav [Class "navbar navbar-dark fixed-top bg-dark flex-md-nowrap p-0 shadow"] [
                routerLink [Class "navbar-brand col-sm-3 col-md-2 mr-0"; Attrs ["to" ==> "/dashboard"]] [str "Dashboard"]
                input [Class "form-control form-control-dark w-100"; Attrs ["type" ==> "text"; "placeholder" ==> "Search"; "aria-label" ==> "Search"]] []
                ul [Class "navbar-nav px-3"] [
                    routerLink [Class "nav-item text-nowrap"; Attrs !!["tag" ==> "li"; "to" ==> "/"]] [
                        a [Class "nav-link"; DomProps ["innerHTML" ==> "Back to Site &raquo;"]] []
                    ]
                ]
            ]

            div [Class "container-fluid"] [
                div [Class "row"] [
                    nav [Class "col-md-2 d-none d-md-block bg-light sidebar"] [
                        div [Class "sidebar-sticky"] [
                            ul [Class "nav flex-column"] [
                                routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "#"]] [
                                    a [Class "nav-link"] [
                                        i [Class "fas fa-home fa-fw"] []
                                        str " Dashboard"
                                    ]
                                ]
                                routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "#"]] [
                                    a [Class "nav-link"] [
                                        i [Class "far fa-file fa-fw"] []
                                        str " Orders"
                                    ]
                                ]
                                routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "#"]] [
                                    a [Class "nav-link"] [
                                        i [Class "fas fa-shopping-cart fa-fw"] []
                                        str " Products"
                                    ]
                                ]
                                routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "#"]] [
                                    a [Class "nav-link"] [
                                        i [Class "fas fa-users fa-fw"] []
                                        str " Customers"
                                    ]
                                ]
                                routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "#"]] [
                                    a [Class "nav-link"] [
                                        i [Class "fas fa-chart-area fa-fw"] []
                                        str " Reports"
                                    ]
                                ]
                                routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "#"]] [
                                    a [Class "nav-link"] [
                                        i [Class "fas fa-bars fa-fw"] []
                                        str " Integrations"
                                    ]
                                ]
                            ]
                        ]
                    ]
                ]
            ]

            h1 [] [str "Dashboard"]
        ]
    )

[<ExportDefault>]
let exports = comp
