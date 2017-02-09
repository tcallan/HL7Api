module Events

open System
open Newtonsoft.Json

// LiteDB Requires a mutable type :(
[<CLIMutable>]
type RawEvent =
    { Id : int32
      AggregateId : Guid
      Version : int32
      UserId : Guid
      (* LiteDB's serialization doesn't handle F# DUs correctly so
         we'll serialize to Json manually since we don't need to query
         on this data *)
      Data : string }

type CreatedQuickViewData =
    { Name : string
      Selectors : string list }

type UpdatedQuickViewData =
    { Selector : string }

type RenamedQuickViewData =
    { Name : string }

type SubscriptionData = string

type EventData =
    | CreatedQuickView of CreatedQuickViewData
    | AddedQuickViewSelector of UpdatedQuickViewData
    | RemovedQuickViewSelector of UpdatedQuickViewData
    | RenamedQuickView of RenamedQuickViewData
    | Subscription of SubscriptionData

type AggregateEvent =
    { Id : int32 
      AggregateId : Guid
      Version : int32 
      UserId : Guid
      Data : EventData }

let private getAggregateData (event : RawEvent) : EventData =
    JsonConvert.DeserializeObject<EventData> (event.Data)

let rawEventToAggregateEvent (event : RawEvent) : AggregateEvent =
    { Id = event.Id
      AggregateId = event.AggregateId 
      Version = event.Version
      UserId = event.UserId
      Data = getAggregateData event }

let private getRawData (event : AggregateEvent) : string =
    JsonConvert.SerializeObject (event.Data)

let aggregateEventToRawEvent (event : AggregateEvent) : RawEvent =
    { Id = event.Id
      AggregateId = event.AggregateId
      Version = event.Version
      UserId = event.UserId
      Data = getRawData event }