namespace HL7Identity.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open Microsoft.AspNetCore.Http.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open Microsoft.AspNetCore.Http
open HL7Identity.Configuration
open Microsoft.Extensions.Options

[<Route("")>]
type HomeController (jwtOptions : IOptions<JwtOptions>) =
    inherit Controller ()

    [<HttpGet>]
    member this.Get () =
        new ContentResult
            (
                Content = this.Html (),
                ContentType = "text/html"
            )

    member private this.Html () =
        let isAuthenticated = (isNull >> not) this.HttpContext.User && this.HttpContext.User.Identity.IsAuthenticated
        let userInfo = if isAuthenticated
                       then sprintf "User: %s" this.User.Identity.Name
                       else "User: Not logged in"
        let logInOut = if isAuthenticated
                       then """<a href="/auth/logout">Logout</a>"""
                       else """<a href="/auth/login">Login</a>"""
        let jwt = if isAuthenticated
                  then HL7Identity.JwtGenerator.generateJwt (jwtOptions.Value) this.HttpContext.User
                  else ""
        let config = sprintf "key: %s, expiration: %f" jwtOptions.Value.SecretKey jwtOptions.Value.Expiration
        sprintf "<div>%s</div><div>%s</div><div>%s</div><div>%s</div>" userInfo logInOut jwt config

[<Route("auth")>]
type AuthController (jwtOptions : IOptions<JwtOptions>) =
    inherit Controller ()

    [<HttpGet("login")>]
    member this.Login () =
        if isNull this.HttpContext.User || not this.HttpContext.User.Identity.IsAuthenticated
        then this.HttpContext.Authentication.ChallengeAsync (OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties (RedirectUri = "/auth/login" ))
        else 
             this.HttpContext.Response.Redirect(sprintf "http://localhost:4200/workspace/0/quick?query=%A" (HL7Identity.JwtGenerator.generateJwt (jwtOptions.Value) this.HttpContext.User))
             Task.CompletedTask

    [<HttpGet("logout")>]
    member this.Logout () =
        async {
            if this.HttpContext.User.Identity.IsAuthenticated
            then
                do! this.HttpContext.Authentication.SignOutAsync OpenIdConnectDefaults.AuthenticationScheme |> Async.AwaitTask
                do! this.HttpContext.Authentication.SignOutAsync CookieAuthenticationDefaults.AuthenticationScheme |> Async.AwaitTask
            return new RedirectResult("http://localhost:4200/workspace/0/quick", false)
        } |> Async.StartAsTask
