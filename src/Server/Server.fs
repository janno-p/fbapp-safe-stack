module FbApp.Server.Server

open FSharp.Control.Tasks.ContextInsensitive
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.Google
open Microsoft.AspNetCore.Authentication.OAuth
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.SpaServices.Webpack
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Configuration.UserSecrets
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open System.IO
open System.Security.Claims
open System.Threading.Tasks

open Giraffe
open Saturn

open Fable.Remoting.Giraffe

open Shared
open FbApp.Server
open FbApp.Server.HttpsConfig
open Giraffe

[<assembly: UserSecretsIdAttribute("d6072641-6e1a-4bbc-bbb6-d355f0e38db4")>]
do ()

module Views =
    open Giraffe.GiraffeViewEngine

    let index node =
        html [_lang "en"] [
            head [] [
                meta [_charset "utf-8"]
                meta [_httpEquiv "X-UA-Compatible"; _content "IE=edge"]
                meta [_name "format-detection"; _content "telephone=no"]
                meta [_name "msapplication-tap-highlight"; _content "no"]
                meta [_name "viewport"; _content "user-scalable=no, initial-scale=1, maximum-scale=1, minimum-scale=1, width=device-width"]

                title [] [rawText "FbApp (SAFE Stack)"]

                link [_rel "stylesheet"; _href "https://fonts.googleapis.com/css?family=Roboto:300,400,500,700|Material+Icons"; _type "text/css"]
                link [_rel "stylesheet"; _href "https://use.fontawesome.com/releases/v5.0.4/css/all.css"]
                link [_rel "stylesheet"; _href "https://stackpath.bootstrapcdn.com/bootstrap/4.1.0/css/bootstrap.min.css"; attr "integrity" "sha384-9gVQ4dYFwwWSjIDZnLEWnxCjeSWFphJiwGPXr1jddIhOegiu1FwO5qRGvFXOdJZ4"; attr "crossorigin" "anonymous"]
                link [_rel "stylesheet"; _href "/Styles/main.css"]
                link [_rel "icon"; _type "image/x-icon"; _href "/Images/safe_favicon.png"]
                link [_rel "shortcut icon"; _type "image/png"; _href "/Images/safe_favicon.png"]
            ]
            body [] [
                noscript [] [rawText "This is your fallback content in case JavaScript fails to load."]
                node
                script [_src "/public/client-bundle.js"] []
            ]
        ]

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

let index: HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! result = ctx |> Prerendering.prerender "../Client/renderOnServer" ""
            match result with
            | Prerendering.PrerenderResult.Content (statusCode, node) ->
                statusCode |> Option.iter (fun code -> ctx.Response.StatusCode <- code)
                return! htmlView (Views.index node) next ctx
            | Prerendering.PrerenderResult.Redirect redirectUrl ->
                return! redirectTo false redirectUrl next ctx
        }

let browserRouter = scope {
    get Urls.index index
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
    use_gzip

    app_config (fun app ->
        let env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>()

        if env.IsDevelopment() then
            app.UseWebpackDevMiddleware(
                    WebpackDevMiddlewareOptions(
                        HotModuleReplacement = true,
                        ConfigFile = "./src/Client/webpack.config.js",
                        ProjectPath = Path.Combine(__SOURCE_DIRECTORY__, "..", "..")))

        app.UseStaticFiles(
                new StaticFileOptions(
                    FileProvider = new PhysicalFileProvider(clientPath),
                    RequestPath = PathString.Empty))
           .UseAuthentication()
    )

    host_config (fun host ->
        host.UseKestrel(fun o -> o.ConfigureEndpoints endpoints)
            .ConfigureAppConfiguration(configureAppConfiguration)
            .ConfigureServices(configureServices)
    )
}

run app
