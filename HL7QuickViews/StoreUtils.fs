module StoreUtils
open LiteDB

let find (predicate: 'a -> bool) (collection : LiteCollection<'a>) =
    collection.Find(predicate)

let findAll (collection : LiteCollection<'a>) =
    collection.FindAll ()

let insert (value : 'a) (collection : LiteCollection<'a>) =
    collection.Insert(value) |> (fun bson -> bson.AsInt32, collection)

let update (value : 'a) (collection : LiteCollection<'a>) =
    collection.Update(value)

let upsert (value : 'a) (collection : LiteCollection<'a>) =
    collection.Upsert(value)