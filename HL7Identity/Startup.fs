namespace HL7Identity

open System
open System.Collections.Generic
open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open Microsoft.AspNetCore.Authentication.OpenIdConnect
open HL7Identity.Configuration

type Startup (env:IHostingEnvironment)=

    let builder = ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile((sprintf "appsettings.%s.json" env.EnvironmentName), true)
                    .AddEnvironmentVariables()

    let configuration = builder.Build()

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices (services:IServiceCollection) =
        // Add framework services.
        services.AddMvc() |> ignore

        services.Configure<JwtOptions>(configuration.GetSection "Jwt") |> ignore

        services.AddAuthentication(fun sharedOptions -> sharedOptions.SignInScheme <- CookieAuthenticationDefaults.AuthenticationScheme) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app:IApplicationBuilder, env:IHostingEnvironment, loggerFactory: ILoggerFactory) =
    
        loggerFactory
            .AddConsole(configuration.GetSection("Logging"))
            .AddDebug()
            |> ignore

        app.UseCookieAuthentication(new CookieAuthenticationOptions ()) |> ignore

        app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            (
                ClientId = configuration.["AzureAd:ClientId"],
                Authority = "https://login.microsoftonline.com/common",
                ResponseType = OpenIdConnectResponseType.IdToken,
                TokenValidationParameters = new TokenValidationParameters
                    (
                        ValidateIssuer = false,
                        NameClaimType = "name"
                    )
            )
        ) |> ignore

        app.UseMvc() |> ignore