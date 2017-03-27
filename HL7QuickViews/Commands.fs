module Commands

open Events
open LiteDB
open System
open EventStore
open Result
open ViewHandlers

type CreateQuickViewData =
    { Name : string
      Selectors : string list }

type UpdateQuickViewData =
    { Selector : string }

type RenameQuickViewData =
    { Name : string }

type CommandData =
    | CreateQuickView of CreateQuickViewData
    | DeleteQuickView
    | AddQuickViewSelector of UpdateQuickViewData
    | RemoveQuickViewSelector of UpdateQuickViewData
    | RenameQuickView of RenameQuickViewData

type Command =
    { AggregateId : Guid
      UserId : Guid
      UserName: string
      Data : CommandData }

type CommandResult = Result<AggregateEvent, string>

(*
Event helper functions
*)
let isNewAggregate (events : AggregateEvent list) : Result<AggregateEvent list, string> =
    match events with
    | [] -> Success events
    | _ -> Failure "Aggregate already exists"

let isExistingAggregate (events : AggregateEvent list) : Result<AggregateEvent list, string> =
    match events with
    | [] -> Failure "Aggregate does not exist"
    | _ -> Success events

let nextVersion (events : AggregateEvent list) : int32 =
    events
    |> List.map (fun e -> e.Version)
    |> List.max
    |> (+) 1

let makeEvent (data : EventData) (version: int32) : AggregateEvent =
    // Shorthand for creating an event
    // `handle` will ensure AggregateId and UserId get set to the appropriate values
    { Id = 0
      AggregateId = Guid.Empty
      Version = version 
      UserId = Guid.Empty
      UserName = ""
      Data = data }

(* 
Handler functions
*)

let handleCreateQuickView (data : CreateQuickViewData) (events : AggregateEvent list) : CommandResult =
    result {
        let! _ = isNewAggregate events
        let data = CreatedQuickView { Name = data.Name
                                      Selectors = data.Selectors }
        return makeEvent data 0
    }

let handleDeleteQuickView (events : AggregateEvent list) : CommandResult =
    result {
        let! events' = isExistingAggregate events
        let version = events' |> nextVersion
        let data = DeletedQuickView
        return makeEvent data version
    }

let handleAddQuickViewSelector (data : UpdateQuickViewData) (events : AggregateEvent list) : CommandResult =
    result {
        let! events' = isExistingAggregate events
        let version = events' |> nextVersion
        let data = AddedQuickViewSelector { Selector = data.Selector }

        return makeEvent data version
    }

let handleRemoveQuickViewSelector (data : UpdateQuickViewData) (events : AggregateEvent list) : CommandResult =
    result {
        let! events' = isExistingAggregate events
        let version = events' |> nextVersion
        let data = RemovedQuickViewSelector { Selector = data.Selector }

        return makeEvent data version
    }

let handleRenameQuickView (data : RenameQuickViewData) (events : AggregateEvent list) : CommandResult =
    result {
        let! events' = isExistingAggregate events
        let version = events' |> nextVersion
        let data = RenamedQuickView { Name = data.Name }

        return makeEvent data version
    }

type CommandHandler = AggregateEvent list -> CommandResult

let getHandler (command : Command) : CommandHandler =
    match command.Data with
    | CreateQuickView data -> handleCreateQuickView data
    | DeleteQuickView -> handleDeleteQuickView
    | AddQuickViewSelector data -> handleAddQuickViewSelector data
    | RemoveQuickViewSelector data -> handleRemoveQuickViewSelector data
    | RenameQuickView data -> handleRenameQuickView data

let addIds (aggregateId : Guid) (userId : Guid) (userName : string) (event : AggregateEvent) =
    { event with
        AggregateId = aggregateId
        UserName = userName
        UserId = userId }

let handle (command : Command) (db : LiteDatabase) : CommandResult =
    result {
        // find the appropriate handler for the command
        let handler = getHandler command

        let! commandEvent =
            db
            |> readAggregateEvents command.AggregateId
            |> handler
            |> Result.map (addIds command.AggregateId command.UserId command.UserName)

        let eventId = db |> persistEvent commandEvent

        let event = { commandEvent with Id = eventId }

        updateViews db event

        return event
    }