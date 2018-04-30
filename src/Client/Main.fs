module FbApp.Client.Main

open Fable.Core.JsInterop
open Fable.Helpers.Vue
open Fable.Import.Vue
open FbApp.Client.Router

let Vue: Vue = importDefault "vue"

Vue.config.productionTip <- false

let private appOptions = createEmpty<ComponentOptions>
appOptions.el <- !^ "#app"
appOptions.router <- router
appOptions.render <- (fun h -> com App.comp)
appOptions.store <- FbApp.Store.Store.store |> unbox

let app = Vue.Create(appOptions)
