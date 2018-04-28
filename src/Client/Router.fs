module FbApp.Client.Router

open Fable.Core.JsInterop
open Fable.Import

let Vue: Vue = importDefault "vue"
let VueRouter: VueRouter = importDefault "vue-router"

Vue.Use(VueRouter)

let private options = createEmpty<RouterOptions>
options.routes <-
    [|
        (
            let r = createEmpty<RouteConfig>
            r.component' <- (fun () -> importDynamic "./Home.fs")
            r.path <- "/"
            r
        )

        (
            let r = createEmpty<RouteConfig>
            r.component' <- (fun () -> importDynamic "./Dashboard.fs")
            r.path <- "/dashboard"
            r
        )

        (
            let r = createEmpty<RouteConfig>
            r.component' <- (fun () -> importDynamic "./Messages.fs")
            r.path <- "/about"
            r.alias <- !^ "/contact"
            r
        )
    |]

let router = VueRouter.Create(Some(options))
