module ViewHandlers
open Events
open Views
open LiteDB
open ViewStore

let createQuickView (event : AggregateEvent) (data : CreatedQuickViewData) : QuickView =
    { Id = 0
      AggregateId = event.AggregateId
      Version = 0
      UserId = event.UserId
      UserName = event.UserName
      IsDeleted = false
      Name = data.Name
      Selectors = data.Selectors }

let getQuickView (event : AggregateEvent) (db : LiteDatabase) =
    db |> getQuickView event.AggregateId

let getQuickViewsForUser (event : AggregateEvent) (db : LiteDatabase) =
    db |> getQuickViewsForUser event.UserId

let addSelector (selector: string) (view: QuickView) =
    { view with Selectors = view.Selectors @ [selector] }

let addQuickViewSelector (data : UpdatedQuickViewData) (view : QuickView option) =
    view |> Option.map (addSelector data.Selector)

let removeSelector (selector : string) (view : QuickView) =
    { view with Selectors = view.Selectors |> List.filter (fun s -> s <> selector) }

let removeQuickViewSelector (data : UpdatedQuickViewData) (view : QuickView option) =
    view |> Option.map (removeSelector data.Selector)

let renameQuickView (data : RenamedQuickViewData) (view : QuickView option) =
    view |> Option.map (fun v -> {v with Name = data.Name})

let deleteQuickView (view: QuickView  option) =
    view |> Option.map (fun v -> {v with IsDeleted = true})

let private updateQuickView (db : LiteDatabase) (event : AggregateEvent) =
    match event.Data with
    | CreatedQuickView data -> createQuickView event data |> Some
    | DeletedQuickView -> getQuickView event db |> deleteQuickView 
    | AddedQuickViewSelector data -> getQuickView event db |> addQuickViewSelector data
    | RemovedQuickViewSelector data -> getQuickView event db |> removeQuickViewSelector data
    | RenamedQuickView data -> getQuickView event db |> renameQuickView data
    | _ -> None // don't care about other event types
    |> Option.map (fun view -> {view with Version = event.Version})
    |> Option.map (persistQuickView db)
    |> ignore


type ViewHandler = LiteDatabase -> AggregateEvent -> unit

let private handlers : ViewHandler list = [
    updateQuickView
]

let updateViews (db : LiteDatabase) (event : AggregateEvent) =
    handlers |> List.map (fun f -> f db event) |> ignore