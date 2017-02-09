namespace HL7QuickViews

open System.Threading.Tasks
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Text
open LiteDB


type Startup (env : IHostingEnvironment) =

    let builder = ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json", true, true)
                    .AddJsonFile((sprintf "appsettings.%s.json" env.EnvironmentName), true)
                    .AddEnvironmentVariables()

    let configuration = builder.Build()

    let tokenValidationParameters =
        new TokenValidationParameters
            (
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey (Encoding.ASCII.GetBytes configuration.["Jwt:SecretKey"]),

                ValidateLifetime = true,

                ValidateAudience = false,
                ValidateIssuer = false
            )

    /// This method gets called by the runtime. Use this method to add services to the container.
    /// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
    member this.ConfigureServices (services : IServiceCollection) = 
        // Add framework services.
        services.AddMvc() |> ignore

        // Add database connection
        services.AddSingleton<LiteDatabase> (Data.getDbInstance configuration.["Data:ConnectionString"]) |> ignore

    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure (app : IApplicationBuilder, env : IHostingEnvironment, loggerFactory : ILoggerFactory ) =
        loggerFactory
            .AddConsole(configuration.GetSection("Logging"))
            .AddDebug()
            |> ignore

        app.UseJwtBearerAuthentication(new JwtBearerOptions
            (
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters,
                SaveToken = true
            )
        ) |> ignore

        app.UseMvc() |> ignore    