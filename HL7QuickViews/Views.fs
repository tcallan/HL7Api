module Views
open System
open Newtonsoft.Json

type QuickView =
    { Id : int32
      AggregateId : Guid
      Version : int32
      UserId : Guid
      Name : string
      Selectors: string list }

// LiteDB requires a mutable instance :(
[<CLIMutable>]
type RawQuickView =
    { Id : int32
      AggregateId : Guid
      Version : int32
      UserId : Guid
      Name : string
      Selectors: string array }

let toRawQuickView (view : QuickView) : RawQuickView =
    { Id = view.Id
      AggregateId = view.AggregateId
      Version = view.Version 
      UserId = view.UserId
      Name = view.Name
      Selectors = view.Selectors |> List.toArray }

let fromRawQuickView (view : RawQuickView) : QuickView =
    { Id = view.Id
      AggregateId = view.AggregateId
      Version = view.Version 
      UserId = view.UserId
      Name = view.Name
      Selectors = view.Selectors |> Array.toList }