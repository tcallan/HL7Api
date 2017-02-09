module ViewStore
open LiteDB
open Views
open Events
open System
open StoreUtils

let quickView (db : LiteDatabase) =
    db.GetCollection<RawQuickView>("quickView")

let init (db : LiteDatabase) =
    (db |> quickView).EnsureIndex(fun e -> e.AggregateId) |> ignore

let private findQuickView (id : Guid) (collection : LiteCollection<RawQuickView>) =
    collection.Find(Query.EQ("AggregateId", new BsonValue(id)))

let getQuickView (id : Guid) (db : LiteDatabase) =
    db
    |> quickView
    |> findQuickView id
    |> Seq.tryHead
    |> Option.map fromRawQuickView

let getAllQuickViews (db : LiteDatabase) =
    db
    |> quickView
    |> findAll
    |> Seq.toList

let private insertQuickView (view : QuickView) (db : LiteDatabase) =
    db
    |> quickView
    |> insert (toRawQuickView view)
    |> ignore

let private updateQuickView (view : QuickView) (db : LiteDatabase) =
    db
    |> quickView
    |> update (toRawQuickView view)
    |> ignore

let persistQuickView (db : LiteDatabase) (view : QuickView) =
    db
    |> quickView
    |> upsert (toRawQuickView view)