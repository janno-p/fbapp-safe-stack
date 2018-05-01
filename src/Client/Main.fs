module FbApp.Client.Main

open Fable.Core
open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue
open FbApp.Client.Router
open FbApp.Client.Store.Store
open FbApp.Client.Store.User
open Shared

let Vue: Vue = importDefault "vue"

[<Import("sync", "vuex-router-sync")>]
let sync: System.Action<Store<obj>, VueRouter> = jsNative

Vue.config.productionTip <- false

type IContext =
    abstract baseUrl: string
    abstract request: IRequest
    abstract isAuthenticated: bool

[<Pojo>]
type AppInfo = {
    main: Vue
    router: VueRouter
    store: Store<IRootState>
}

let createApp (context: IContext) =
    // Api.init(baseUrl, request)
    
    let router = FbApp.Client.Router.router // createRouter()
    let store = createStore()
    
    sync.Invoke(store |> unbox, router)
    
    let options = createEmpty<ComponentOptions>
    options.el <- !^ "#app"
    options.router <- router
    options.render <- (fun h -> com App.comp)
    options.store <- store |> unbox
    
    let app = Vue.Create(options)
    
    user.commitSetAuthenticated(context.isAuthenticated)
    
    { main = app
      router = router
      store = store }
