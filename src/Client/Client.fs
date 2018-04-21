module Client

open Fable.Core.JsInterop
open Fable.Import
open Shared

module Server = 
    open Fable.Remoting.Client

    let api : ICounterProtocol = 
        Proxy.remoting<ICounterProtocol> {
            use_route_builder Route.builder
        }

let Vue: Vue = importDefault "vue"

let app = 
    Vue.Create(
        createObj [
            "el" ==> "#app"
            "render" ==> (fun h -> h "h1" createEmpty<obj> [| "Hello World" |])
        ]
    )
