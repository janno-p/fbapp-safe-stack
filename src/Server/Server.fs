open Microsoft.AspNetCore.Hosting
open System.IO
open System.Threading.Tasks

open Giraffe
open Saturn

open Fable.Remoting.Giraffe

open Shared
open FbApp.Server.HttpsConfig

let clientPath = Path.Combine("..", "Client") |> Path.GetFullPath

let getInitCounter () : Task<Counter> = task { return 42 }

let browserRouter = scope {
    get "/" (htmlFile (Path.Combine(clientPath, "index.html")))
}

let server = {
    getInitCounter = getInitCounter >> Async.AwaitTask
}

let webApp =
    remoting server {
        use_route_builder Route.builder
    }

let mainRouter = scope {
    forward "" browserRouter
    forward "" webApp
}

let endpoints = [
    { EndpointConfiguration.Default with
        Port = Some 8085 }
    { EndpointConfiguration.Default with
        Port = Some 44340
        Scheme = Https
        FilePath = Some (Path.Combine(__SOURCE_DIRECTORY__, "..", "FbApp.pfx")) }
]

let app = application {
    router mainRouter
    memory_cache
    use_static clientPath
    use_gzip
    host_config (fun host -> host.UseKestrel(fun o -> o.ConfigureEndpoints endpoints))
}

run app
