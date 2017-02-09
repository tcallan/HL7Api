module EventStore

open Events
open System
open LiteDB
open StoreUtils

let events (db : LiteDatabase) =
    db.GetCollection<RawEvent>("events")

let init (db : LiteDatabase) =
    (db |> events).EnsureIndex(fun e -> e.AggregateId) |> ignore

let private findByAggregateId (id : Guid) (collection : LiteCollection<RawEvent>) =
    collection.Find(Query.EQ("AggregateId", new BsonValue(id)))

let private aggregateExists (id : Guid) (collection : LiteCollection<RawEvent>) =
    collection.Exists(Query.EQ("AggregateId", new BsonValue(id)))

let readAggregateEvents (id : Guid) (db : LiteDatabase) : AggregateEvent list =
    db
    |> events
    |> findByAggregateId id
    |> Seq.map rawEventToAggregateEvent
    |> Seq.toList

let persistEvent (event : AggregateEvent) (db : LiteDatabase) : int32 = 
    db
    |> events
    |> insert (aggregateEventToRawEvent event)
    |> fst