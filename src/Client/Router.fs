module FbApp.Client.Router

open Fable.Core.JsInterop
open Fable.Import.Vue

let Vue: Vue = importDefault "vue"
let VueRouter: VueRouter = importDefault "vue-router"

Vue.Use(VueRouter)

let private options = createEmpty<RouterOptions>
options.linkExactActiveClass <- "active"
options.mode <- RouterMode.History
options.routes <-
    [|
        (
            let r = createEmpty<RouteConfig>
            r.component' <- (fun () -> importDynamic "./Pages/Home.fs")
            r.path <- "/"
            r
        )

        (
            let r = createEmpty<RouteConfig>
            r.component' <- (fun () -> importDynamic "./Pages/Dashboard.fs")
            r.path <- "/dashboard"
            r
        )

        (
            let r = createEmpty<RouteConfig>
            r.component' <- (fun () -> importDynamic "./Pages/Messages.fs")
            r.path <- "/about"
            r.alias <- !^ "/contact"
            r
        )
    |]

let router = VueRouter.Create(Some(options))
