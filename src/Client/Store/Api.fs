module FbApp.Client.Store.Api

open Fable.Core.JsInterop
open Fable.Remoting.Client
open Shared

let Noty: obj = importDefault "noty"

let api endpoint request : ICounterProtocol = 
    Proxy.remoting<ICounterProtocol> {
        at_endpoint endpoint

        use_error_handler (
            match request with
            | Some _ -> ignore
            | None ->
                (fun e ->
                    let n =
                        createNew
                            Noty
                            (createObj [
                                "layout" ==> "center"
                                "modal" ==> true
                                "text" ==> e.error
                                "theme" ==> "mint"
                                "type" ==> "error"
                            ])
                    n?show() |> ignore
                )
        )

        use_route_builder Route.builder
    }
