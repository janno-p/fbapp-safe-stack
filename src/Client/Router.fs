module FbApp.Client.Router

open Fable.Core.JsInterop
open Fable.Import
open FbApp.Components

let Vue: Vue = importDefault "vue"
let VueRouter: VueRouter = importDefault "vue-router"

Vue.Use(VueRouter)

let private options = createEmpty<RouterOptions>
options.routes <-
    [|
        (let r = createEmpty<RouteConfig> in
         r.path <- "/"
         r.name <- "Home"
         r.component' <- Home.comp
         r)
    |]

let router = VueRouter.Create(Some(options))
