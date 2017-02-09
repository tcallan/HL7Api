module Result

type Result<'a, 'b> =
    | Success of 'a
    | Failure of 'b

let map f x =
    match x with
    | Success value -> f value |> Success
    | Failure value -> Failure value

type ResultBuilder () =
    member this.Bind (x, f) =
        match x with
        | Success value -> f value
        | Failure value -> Failure value

    member this.Return x =
        Success x

let result = new ResultBuilder ()
