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
