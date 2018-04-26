module FbApp.Components.Home

open Fable.Core.JsInterop
open Fable.Import
open Fable.Helpers.Vue

let Vue: Vue = importDefault "vue"

let private options = createEmpty<ComponentOptions>
options.render <- (fun h -> h1 [] [str "Hello World"])

let comp = Vue.Extend(options)
