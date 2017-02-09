namespace Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Authorization
open LiteDB
open System
open Commands
open ViewStore
open EventStore
open StoreUtils

module Helpers =
    open System.IdentityModel.Tokens.Jwt
    open System.Collections.Generic
    open System.Security.Claims

    let getClaim claimType (claims : IEnumerable<Claim>) =
        let maybeClaim = claims |> Seq.tryFind (fun c -> c.Type = claimType)
        match maybeClaim with
        | Some claim -> claim.Value
        | None -> String.Empty

    let getUserId claims =
        claims
        |> getClaim "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
        |> Guid.Parse

(*
Note: the controller types themselves need to not be in a module in order to be properly detected
*)

[<Route("")>]
type DefaultController () =
    inherit Controller ()

    [<HttpGet>]
    member this.Get () = ()

[<Route("command/quickview")>]
type QuickViewCommandContoller (db : LiteDatabase) =
    inherit Controller ()

    [<Authorize>]
    [<HttpPost("{id}/create")>]
    member this.Create (id : Guid, [<FromBody>]data : CreateQuickViewData) =
        let command =
            { AggregateId = id
              UserId = Helpers.getUserId this.User.Claims
              Data = CreateQuickView data }
        
        handle command db

    [<Authorize>]
    [<HttpPost("{id}/addselector")>]
    member this.AddSelector (id : Guid, [<FromBody>]data : UpdateQuickViewData) =
        let command =
            { AggregateId = id
              UserId = Helpers.getUserId this.User.Claims
              Data = AddQuickViewSelector data }

        handle command db

    [<Authorize>]
    [<HttpPost("{id}/removeselector")>]
    member this.RemoveSelector (id : Guid, [<FromBody>]data : UpdateQuickViewData) =
        let command =
            { AggregateId = id 
              UserId = Helpers.getUserId this.User.Claims
              Data = RemoveQuickViewSelector data }

        handle command db

    [<Authorize>]
    [<HttpPost("{id}/rename")>]
    member this.Rename (id : Guid, [<FromBody>]data : RenameQuickViewData) =
        let command =
            { AggregateId = id 
              UserId = Helpers.getUserId this.User.Claims
              Data = RenameQuickView data }

        handle command db

[<Route("query/quickview")>]
type QuickViewQueryController (db : LiteDatabase) =
    inherit Controller ()

    [<Authorize>]
    [<HttpGet("byid/{id}")>]
    member this.Get (id : Guid) : IActionResult =
        match db |> getQuickView id with
        | Some view -> new ObjectResult (view) :> IActionResult
        | None -> this.NotFound () :> IActionResult

    [<Authorize>]
    [<HttpGet("all")>]
    member this.GetAll () =
        db |> quickView |> findAll |> Seq.toList