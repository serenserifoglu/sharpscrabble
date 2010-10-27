﻿namespace Scrabble.Core.Types

open System
open Scrabble.Core
open Scrabble.Core.Config
open Scrabble.Core.Squares
open Scrabble.Core.Helper
open Scrabble.Dictionary

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

    member this.HasNeighboringTile(c:Coordinate) =
        c.Neighbors() |> Seq.exists (fun n -> this.HasTile(n))

    member this.PrettyPrint() = 
        printf "   "
        for j in 0 .. (Array2D.length2 grid) - 1 do
            printf "%2i " j
        printfn ""
        for i in 0 .. (Array2D.length1 grid) - 1 do
            printf "%2i " i
            for j in 0 .. (Array2D.length2 grid) - 1 do
                let s = Array2D.get grid j i
                if s.Tile <> null then
                    let tile = s.Tile :?> Tile
                    printf " %c " tile.Letter
                else
                    printf " _ "
            printfn ""

and GameState(players:Player list) = 
    let bag = Bag()
    let board = Board()
    let mutable moveCount = 0
    let wordLookup = lazy(WordLookup())
    //Properties
    member this.TileBag with get() = bag
    member this.PlayingBoard with get() = board
    member this.MoveCount with get() = moveCount
    member this.IsOpeningMove with get() = moveCount = 0
    member this.Players with get() = players
    member this.Dictionary with get() = wordLookup.Value
    //Public Methods
    member this.NextMove() =
        moveCount <- moveCount + 1

/// A singleton that will represent the game board, bag of tiles, players, move count, etc.
and Game() = 
    static let instance = lazy(GameState([ HumanPlayer("Will") :> Player; ComputerPlayer("Com") :> Player ])) //Pretty sweet, huh? Hard coding stuff...
    static member Instance with get() = instance.Value

/// A player's move is a set of coordinates and tiles. This will throw if the move isn't valid.
/// That is, if the tiles aren't layed out properly (not all connected, the word formed doesn't "touch" any other tiles - with the exception of the first word)
/// and if there is a "run" of connected tiles that doesn't form a valid word
and Move(letters:Map<Coordinate, Tile>) = 
    let sorted = letters |> Seq.sortBy ToKey |> Seq.toList
    let first = sorted |> Seq.head |> ToKey
    let last = sorted |> Seq.skip (sorted.Length - 1) |> Seq.head |> ToKey
    let range = 
        try
            Coordinate.Between(first, last)
        with 
            | UnsupportedCoordinateException(msg) -> raise (InvalidMoveException(msg))
    let orientation = 
        if first.X = last.X then
            Orientation.Vertical
        else
            Orientation.Horizontal

    //Private methods
    let CheckMoveOccupied(c:Coordinate) =
            letters.ContainsKey(c) || Game.Instance.PlayingBoard.HasTile(c)
    let Opposite(o:Orientation) =
        match o with
        | Orientation.Horizontal -> Orientation.Vertical
        | _ -> Orientation.Horizontal
    let IsAligned() = 
        if letters.Count <= 1 then
            true
        else
            let c0 = (Seq.head letters) |> ToKey // note: added the helper method "ToKey" to replace this: (fun pair -> pair.Key)
            let v = letters |> Seq.map (fun pair -> pair.Key.X) |> Seq.forall (fun x -> c0.X = x)
            let h = letters |> Seq.map (fun pair -> pair.Key.Y) |> Seq.forall (fun y -> c0.Y = y)
            v || h
    let IsConsecutive() =
        range |> Seq.forall (fun c -> CheckMoveOccupied(c))
    let IsConnected() = 
        range |> Seq.exists (fun c -> Game.Instance.PlayingBoard.HasTile(c) || Game.Instance.PlayingBoard.HasNeighboringTile(c))
    let ContainsStartSquare() = 
        letters.ContainsKey(ScrabbleConfig.StartCoordinate)
    let ValidPlacement() = 
        IsAligned() && IsConsecutive() && ((Game.Instance.IsOpeningMove && ContainsStartSquare()) || (not Game.Instance.IsOpeningMove && IsConnected()))
    let ComputeRuns() : Run list = 
        let alt = Opposite(orientation)
        let alternateRuns = sorted |> Seq.map (fun pair -> Run(pair.Key, alt, letters)) |> Seq.filter (fun r -> r.Length > 1) |> Seq.toList
        Run(first, orientation, letters) :: alternateRuns
    let ValidRuns(runs: Run list) = 
        runs |> Seq.forall (fun r -> r.IsValid())
    let ComputeScore(runs : Run list) =
        runs |> List.sumBy (fun r -> r.Score())

    let score = 
        if ValidPlacement() then
            //make sure every sequence of tiles with length > 1 formed by this move is a valid word
            let runs = ComputeRuns()
            if ValidRuns(runs) then
                ComputeScore(runs)
            else
                raise (InvalidMoveException("One or more invalid words were formed by this move."))
        else
            raise (InvalidMoveException("Move violates positioning rules (i.e. not connected to other tiles)."))

    member this.Orientation with get() = orientation
    member this.Letters with get() = letters
    member this.Score with get() = score
    

/// A Run is a series of connected letters in a given direction. This type takes a location and direction and constructs a map of connected tiles to letters in the given direction.
and Run(c:Coordinate, o:Orientation, moveLetters:Map<Coordinate, Tile>) = 
    let GetTileFromMove(c:Coordinate) = 
        match moveLetters.TryFind c with
        | Some t -> t :> obj
        | None -> Game.Instance.PlayingBoard.Get(c).Tile
    let rec Check(c:Coordinate, o:Orientation, increment) =
        if not (c.IsValid()) then
            []
        else 
            let s = Game.Instance.PlayingBoard.Get(c)
            let t = GetTileFromMove(c)
            if t <> null then
                let next = increment(c, o)
                (s, t) :: Check(next, o, increment)
            else
                []
            
    let prevSquares = Check(c, o, (fun (c:Coordinate, o:Orientation) -> c.Prev(o)))
    let nextSquares = Check(c.Next(o), o, (fun (c:Coordinate, o:Orientation) -> c.Next(o)))
    let squares = (List.rev prevSquares) @ nextSquares 

    member this.Orientation with get() = o
    member this.Squares with get() = squares
    member this.Length with get() = squares.Length
    member this.ToWord() =
        squares |> List.map (fun (s, t) -> t :?> Tile) |> List.map (fun t -> t.Letter.ToString()) |> List.reduce (fun s0 s1 -> s0 + s1)
    member this.IsValid() = 
        Game.Instance.Dictionary.IsValidWord(this.ToWord())
    member this.Score() =
        let wordMult = squares |> List.map (fun (s, t) -> s.WordMultiplier) |> List.reduce (fun a b -> a * b)
        let letterScore = squares |> List.map (fun (s, t) -> (s, t :?> Tile)) |> List.map (fun (s, t) ->  s.LetterMultiplier * t.Score ) |> List.sum
        wordMult * letterScore