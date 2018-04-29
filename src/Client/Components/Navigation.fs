module FbApp.Components.Navigation

open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue

// import Api from "../store/api";
// import user from "../store/user";

type NavigationComputed () =
    member __.isAuthenticated
        with get() = true // user.state.isAuthenticated
    member __.signInUri
        with get() = "" // Api.url(`Account/Login?redirectUri=${this.$route.fullPath}`)
    member __.signOutUri
        with get() = "" // Api.url("Account/Logout")

let comp = createEmpty<ComponentOptions>
comp.computed <- NavigationComputed() |> toComputed
comp.render <-
    (fun h ->
        let c = jsThis<NavigationComputed>
        nav [Class "navbar navbar-expand-lg navbar-dark bg-dark fixed-top"] [
            div [Class "container"] [
                routerLink [Class "navbar-brand"; Attrs !!["to" ==> "/"]] [
                    i [Class "fas fa-futbol"] []
                    span [DomProps !!["innerHTML" ==> "&nbsp;Ennustusmäng"]] []
                ]

                button [Class "navbar-toggler"; Attrs !!["type" ==> "button"; "data-toggle" ==> "collapse"; "data-target" ==> "#navbarItems"]] [
                    span [Class "navbar-toggler-icon"] []
                ]

                div [Class "collapse navbar-collapse"; Attrs !!["id" ==> "navbarItems"]] [
                    if c.isAuthenticated then
                        yield ul [Class "navbar-nav mr-auto"] [
                            routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "/about"]] [
                                a [Class "nav-link"] [str "About"]
                            ]
                            routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "/contact"]] [
                                a [Class "nav-link"] [str "Contact"]
                            ]
                        ]

                    yield ul [Class "navbar-nav ml-auto"] [
                        if c.isAuthenticated then
                            yield routerLink [Class "nav-item"; Attrs !!["tag" ==> "li"; "to" ==> "/dashboard"]] [
                                a [Class "nav-link"; Attrs !!["title" ==> "Juhtpaneel"]] [
                                    i [Class "fas fa-cog"] []
                                ]
                            ]
                            yield li [Class "nav-item"; Style "margin-left: 1rem;"] [
                                a [Class "nav-link"; Props !!["href" ==> c.signOutUri]; Attrs !!["title" ==> "Logi välja"]] [
                                    i [Class "fas fa-sign-out-alt"] []
                                ]
                            ]
                        else
                            yield li [Class "nav-item"] [
                                a [Class "nav-link"; Props !!["href" ==> c.signInUri]; Attrs !!["title" ==> "Logi sisse kasutades oma Google kontot"]] [
                                    i [Class "fab fa-google"] []
                                    span [DomProps !!["innerHTML" ==> "&nbsp; Logi sisse"]] []
                                ]
                            ]
                    ]
                ]
            ]
        ]
    )
