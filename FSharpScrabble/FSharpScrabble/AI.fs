module Scrabble.Core.AI

open Scrabble.Core.Config
open Scrabble.Core.Squares
open Scrabble.Core.Types
open Scrabble.Dictionary


type MoveGenerator(lookup:Scrabble.WordLookup.WordLookup) =

    // first word of the game must include (7,7)
    let PossibleStarts(word:string, o:Orientation) =
        let highestStart:int = max 0 (7 - word.Length + 1)

        let starts = [for i in highestStart .. 7 do
                        match o with
                            Orientation.Vertical -> yield new Coordinate(7, i)
                            | _ -> yield new Coordinate(i, 7)]
        starts

    let CalculateFirstMove(tilesInHand:seq<Tile>) = 
        let possibleWords = lookup.FindAllWords(tilesInHand |> Seq.map (fun w -> w.Letter) |> Seq.toList)
        let orientations:seq<Orientation> = Seq.cast(System.Enum.GetValues(typeof<Orientation>))

        let moves = 
            [| for o in orientations do
                   for word in possibleWords do
                       for start in PossibleStarts(word, o) do
                           let m = [| for i in 0 .. word.Length-1 do
                                          yield (start.Next(o, i), new Tile(word.ToUpper().[i]))
                                   |]
                           yield new Move(Map.ofSeq m) 
            |]

        moves |> Seq.maxBy (fun t -> t.Score) 

    let CalculateBestMove(tilesInHand:seq<Tile>, b:Board) = 
        new Move(Map.empty)

    member this.DetermineBestMove (tilesInHand:seq<Tile>, b:Board) = 
        let move = match b.OccupiedSquares().IsEmpty with
                        true -> CalculateFirstMove(tilesInHand)
                        | false -> CalculateBestMove(tilesInHand, b)
        move