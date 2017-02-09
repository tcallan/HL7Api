module Data

open LiteDB

// See https://github.com/mbdavid/LiteDB/wiki/Connection-String for acceptable values
let getDbInstance (connectionString : string) =
    let db = new LiteDatabase (connectionString)

    EventStore.init db
    ViewStore.init db

    db