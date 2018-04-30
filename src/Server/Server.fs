module FbApp.Server.Server

open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.Google
open Microsoft.AspNetCore.Authentication.OAuth
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration.UserSecrets
open System.IO
open System.Security.Claims
open System.Threading.Tasks

open Giraffe
open Saturn

open Fable.Remoting.Giraffe

open Shared
open FbApp.Server
open FbApp.Server.HttpsConfig

[<assembly: UserSecretsIdAttribute("d6072641-6e1a-4bbc-bbb6-d355f0e38db4")>]
do ()

let clientPath = Path.Combine("..", "Client") |> Path.GetFullPath

let challenge (scheme: string) (redirectUri: string) : HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            do! ctx.ChallengeAsync(scheme, AuthenticationProperties(RedirectUri = redirectUri))
            return! next ctx
        }

let googleAuth: HttpHandler = challenge GoogleDefaults.AuthenticationScheme "/"

let refreshToken: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) -> task {
        ctx |> XsrfToken.refresh
        return! next ctx
    }

let logout: HttpHandler =
    signOut CookieAuthenticationDefaults.AuthenticationScheme
    >=> refreshToken
    >=> redirectTo false Urls.index

let getInitCounter () : Task<Counter> = task { return 42 }

let browserRouter = scope {
    get Urls.index (htmlFile (Path.Combine(clientPath, "index.html")))
    get Urls.login googleAuth
    get Urls.logout logout
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
        Port = Some 5000 }
    { EndpointConfiguration.Default with
        Port = Some 5001
        Scheme = Https
        FilePath = Some (Path.Combine(__SOURCE_DIRECTORY__, "..", "FbApp.pfx")) }
]

let configureServices (ctx: WebHostBuilderContext) (services: IServiceCollection) =
    services.AddAntiforgery(fun o -> o.HeaderName <- XsrfToken.CookieName)
    |> ignore

    services
        .AddAuthentication(fun o ->
            o.DefaultAuthenticateScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            o.DefaultScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            o.DefaultChallengeScheme <- CookieAuthenticationDefaults.AuthenticationScheme
        )
        .AddCookie(fun o ->
            o.LoginPath <- PathString Urls.login
            o.LogoutPath <- PathString Urls.logout
            o.Events <- CookieAuthenticationEvents(
                OnRedirectToLogin = (fun context ->
                    context.Response.ContentType <- "text/plain"
                    context.Response.StatusCode <- StatusCodes.Status401Unauthorized
                    Task.CompletedTask
                ),
                OnSignedIn = (fun context ->
                    context.HttpContext |> XsrfToken.refresh
                    Task.CompletedTask
                )
            )
        )
        .AddGoogle(fun o ->
            let conf = ctx.Configuration
            o.ClientId <- conf.["Authentication:Google:ClientId"]
            o.ClientSecret <- conf.["Authentication:Google:ClientSecret"]
            o.SignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme
            o.Events <- OAuthEvents(
                OnCreatingTicket = (fun context ->
                    context.Identity.Claims
                    |> Seq.tryFind (fun c -> c.Type = ClaimTypes.Email)
                    |> Option.iter (fun claim ->
                        if claim.Value = conf.["Authentication:Google:AdminEmail"] then
                            context.Identity.AddClaim(Claim("IsAdmin", "true")))
                    Task.CompletedTask
                )
            )
        )
        |> ignore

let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =
    config
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(sprintf "appsettings.%s.json" context.HostingEnvironment.EnvironmentName, true)
        .AddEnvironmentVariables()
        .AddUserSecrets<ICounterProtocol>()
    |> ignore

let app = application {
    router mainRouter
    memory_cache
    //use_static clientPath
    use_gzip

    app_config (fun app ->
        app.UseAuthentication()
    )

    host_config (fun host ->
        host.UseKestrel(fun o -> o.ConfigureEndpoints endpoints)
            .ConfigureAppConfiguration(configureAppConfiguration)
            .ConfigureServices(configureServices)
    )
}

run app
