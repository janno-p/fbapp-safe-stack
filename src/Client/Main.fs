module FbApp.Client.Main

open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.Vue
open FbApp.Client.Router

let Vue: Vue = importDefault "vue"

Vue.config.productionTip <- false

let private appOptions = createEmpty<ComponentOptions>
appOptions.el <- !^ "#app"
appOptions.router <- router
appOptions.render <- (fun h -> com App.comp)

let app = Vue.Create(appOptions)
