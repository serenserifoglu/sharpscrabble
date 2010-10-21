module Scrabble.Core.Types

open System
open Scrabble.Core.Config
open Scrabble.Core.Squares
open Scrabble.Core.Helper

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

[<AbstractClass>]
type Player(name:string) =
    let mutable tiles : Tile array = Array.zeroCreate ScrabbleConfig.MaxTiles
    member this.Name with get() = name

type HumanPlayer(name:string) =
    inherit Player(name)

type ComputerPlayer(name:string) = 
    inherit Player(name)

type Board() = 
    let grid : Square[,] = Array2D.init ScrabbleConfig.BoardLength ScrabbleConfig.BoardLength (fun x y -> ScrabbleConfig.BoardLayout (Coordinate(x, y))) 
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
    member this.Put(m:Move) = 
        m.Letters |> Seq.toList |> Seq.iter (fun (pair:Collections.Generic.KeyValuePair<Coordinate, Tile>) -> this.Put(pair.Value, pair.Key))

    member this.OccupiedSquares() : Map<Coordinate, Square> = 
        Map.ofList [ for i in 0 .. (Array2D.length1 grid) - 1 do
                        for j in 0 .. (Array2D.length2 grid) - 1 do
                            let s = Array2D.get grid i j
                            if s.Tile <> null then
                                yield (Coordinate(i, j), s) ]
    member this.PrettyPrint() = 
        printf "   "
        for j in 0 .. (Array2D.length2 grid) - 1 do
            printf "%2i " j
        printfn ""
        for i in 0 .. (Array2D.length1 grid) - 1 do
            printf "%2i " i
            for j in 0 .. (Array2D.length2 grid) - 1 do
                let s = Array2D.get grid i j
                if s.Tile <> null then
                    let tile = s.Tile :?> Tile
                    printf " %c " tile.Letter
                else
                    printf " _ "
            printfn ""

and GameState(players : Player list) = 
    let bag = Bag()
    let board = Board()
    let moveCount = 0
    member this.TileBag with get() = bag
    member this.PlayingBoard with get() = board
    member this.MoveCount with get() = moveCount
    member this.IsOpeningMove with get() = moveCount = 0
    member this.Players with get() = players

/// A singleton that will represent the game board, bag of tiles, players, move count, etc.
and Game() = 
    static let instance = lazy(GameState([ HumanPlayer("Will") :> Player; ComputerPlayer("Com") :> Player ])) //Pretty sweet, huh? Hard coding stuff...
    static member Instance with get() = instance.Value

/// A player's move is a set of coordinates and tiles. This will then validate whether or not the tiles form a valid move
and Move() = 
    let mutable letters : Map<Coordinate, Tile> = Map.empty
    let CheckMoveOccupied(c:Coordinate) =
        letters.ContainsKey(c) || Game.Instance.PlayingBoard.HasTile(c)
    member this.Letters with get() = letters
    member this.AddTile(c:Coordinate, t:Tile) = 
        letters <- letters.Add(c, t)
    member this.IsAligned() = 
        if letters.Count <= 1 then
            true
        else
            let c0 = (Seq.head letters) |> ToKey // note: added the helper method "ToKey" to replace this: (fun pair -> pair.Key)
            let v = letters |> Seq.map (fun pair -> pair.Key.X) |> Seq.forall (fun x -> c0.X = x)
            let h = letters |> Seq.map (fun pair -> pair.Key.Y) |> Seq.forall (fun y -> c0.Y = y)
            v || h
    member this.IsConsecutive() =
        let sorted = letters |> Seq.sortBy ToKey |> Seq.toList
        let first = sorted |> Seq.head |> ToKey
        let last = sorted |> Seq.skip (sorted.Length - 1) |> Seq.head |> ToKey
        Coordinate.Between(first, last) |> Seq.forall (fun c -> CheckMoveOccupied(c))
    //member this.IsConnected() = 
    //    Game.Instance.IsOpeningMove || 

    
        