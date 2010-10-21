module Scrabble.Core.Types

open System
open Scrabble.Core.Config
open Scrabble.Core.Squares

type Tile(letter:char) = 
    let getScore l = 
        match l with
        | 'E' | 'A' | 'I' | 'O' | 'N'| 'R' | 'T' | 'L' | 'S' | 'U' -> 1
        | 'D' | 'G' -> 2
        | 'B' | 'C' | 'M' | 'P' -> 3
        | 'F' | 'H' | 'V' |  'W' | 'Y' -> 4
        | 'K' -> 5
        | 'J' | 'X' -> 8
        | 'Q' | 'Z' -> 10
        | ' ' -> 0
        | _ -> raise (Exception("Only uppercase characters A - Z and a blank space are supported in Scrabble."))
    let score = getScore letter
    member this.Letter with get() = letter
    member this.Score with get() = score
    member this.Print() = printfn "Letter: %c, Score: %i" this.Letter this.Score

type Bag() = 
    let mutable pointer = 0;
    let inventory = 
        ScrabbleConfig.LetterQuantity
        |> Seq.map (fun kv -> Array.create kv.Value (Tile(kv.Key)))
        |> Seq.reduce (fun a0 a1 -> Array.append a0 a1)
        |> Seq.sortBy (fun t -> System.Guid.NewGuid()) //according to Stackoverflow, this is a totally cool way to shuffle
        |> Seq.toArray
    member this.IsEmpty with get() = inventory.Length = pointer
    member this.PrintAll() = inventory |> Seq.iter (fun t -> t.Print())
    member this.PrintRemaining() = 
        for i = pointer to inventory.Length - 1 do
            inventory.[i].Print()
    member this.Take(n:int) = 
        if this.IsEmpty then
            raise (Exception("The bag is empty, you can not take any tiles."))
        let canTake = System.Math.Min(inventory.Length - pointer, n)
        let old = pointer
        pointer <- pointer + canTake
        Array.sub inventory old canTake
    member this.Take() = 
        this.Take(1).[0]

type Board() = 
    let grid = Array2D.init ScrabbleConfig.BoardLength ScrabbleConfig.BoardLength (fun x y -> ScrabbleConfig.BoardLayout (Coordinate(x, y))) 
    member this.Get(c:Coordinate) =
        this.Get(c.X, c.Y)
    member this.Get(x:int, y:int) =
        grid.[x, y]
    member this.HasTile(c:Coordinate) = 
        not (this.Get(c).IsEmpty)
    member this.Put(t:Tile, c:Coordinate) = 
        if not (this.HasTile(c)) then
            this.Get(c).Tile <- t
        else
            raise (Exception("A tile already exists on the square."))
    //Big todo on this next function...yeah...
    //member this.OccupiedSquares() : Map<Coordinate, Square> = 
        
type Move() = 
    let mutable letters : Map<Coordinate, Tile> = Map.empty
    member this.AddTile(c:Coordinate, t:Tile) = 
        letters <- letters.Add(c, t)
    member this.IsAligned() = 
        if letters.Count <= 1 then
            true
        else
            let c0 = (Seq.head letters) |> (fun pair -> pair.Key)
            let v = letters |> Seq.map (fun pair -> pair.Key.X) |> Seq.forall (fun x -> c0.X = x)
            let h = letters |> Seq.map (fun pair -> pair.Key.Y) |> Seq.forall (fun y -> c0.Y = y)
            v || h
    member this.IsConsecutive() =
        let sorted = letters |> Seq.sortBy (fun pair -> pair.Key) |> Seq.toList
        let last = sorted |> Seq.skip (sorted.Length - 1) |> Seq.head
        
        raise (NotImplementedException("not done with this method yet..."))
        

[<AbstractClass>]
type Player(name:string) =
    let mutable tiles : Tile array = Array.zeroCreate ScrabbleConfig.MaxTiles
    member this.Name with get() = name

    //OK this doesn't make any sense...why doesn't this compile?
    //member this.Tiles() = tiles |> Seq.filter (fun t -> t <> null) |> Seq.toList

type HumanPlayer(name:string) =
    inherit Player(name)