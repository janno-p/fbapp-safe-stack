module FbApp.Client.Store.Api

open Fable.Remoting.Client
open Shared

let api : ICounterProtocol = 
    Proxy.remoting<ICounterProtocol> {
        use_route_builder Route.builder
    }
