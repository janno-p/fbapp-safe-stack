module FbApp.Server.Prerendering

open FSharp.Control.Tasks.ContextInsensitive
open Giraffe.GiraffeViewEngine
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Http.Features
open Microsoft.AspNetCore.NodeServices
open Microsoft.AspNetCore.SpaServices.Prerendering
open Microsoft.Extensions.DependencyInjection

type PrerenderResult =
    | Redirect of string
    | Content of int option * XmlNode

let prerender moduleName model (context: HttpContext) = task {
    let env = context.RequestServices.GetRequiredService<IHostingEnvironment>()
    let applicationLifetime = context.RequestServices.GetRequiredService<IApplicationLifetime>()

    let nodeServices =
        match context.RequestServices.GetService<INodeServices>() with
        | null -> NodeServicesFactory.CreateNodeServices(NodeServicesOptions(context.RequestServices))
        | services -> services

    let requestFeature = context.Features.Get<IHttpRequestFeature>()
    let unencodedPathAndQuery = requestFeature.RawTarget
    let request = context.Request

    let! result =
        Prerenderer.RenderToString(
            env.ContentRootPath,
            nodeServices,
            applicationLifetime.ApplicationStopping,
            JavaScriptModuleExport(moduleName),
            sprintf "%s://%s%s" request.Scheme (request.Host.ToString()) unencodedPathAndQuery,
            unencodedPathAndQuery,
            model,
            0,
            request.PathBase.ToString()
        )

    match result.RedirectUrl with
    | null | "" ->
        let node =
            match result.CreateGlobalsAssignmentScript() with
            | null | "" -> result.Html
            | globalsScript -> sprintf "%s<script>%s</script>" result.Html globalsScript
            |> rawText
        return Content(result.StatusCode |> Option.ofNullable, node)
    | redirectUrl -> return Redirect(redirectUrl)
}
