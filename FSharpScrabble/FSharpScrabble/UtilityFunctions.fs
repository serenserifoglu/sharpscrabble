module Scrabble.Core.UtilityFunctions

open Scrabble.Core.Types
open Scrabble.Core.Config

//default utility function, just score the move
let MaximumScore(tiles:TileList, move) = System.Convert.ToDouble(Move(move).Score)


// increase 'score' of words that leave common letters left over
// since we'll then have a better chance at a 7-letter word
let SaveCommon(tiles:TileList, move:Map<Config.Coordinate, Tile>) = 
    let t:Tile[] = 
        [for tile in tiles do yield tile] |> Seq.toArray

    let list = System.Collections.Generic.List(t)
    for item in move do
        list.Remove(item.Value) |> ignore

    let mutable scale = 0

    for tile in list do
        match tile.Letter with
            // per english word frequencies, these are the 7 most common letters
            | 'E' | 'T' | 'A' | 'O' | 'I' | 'N' | 'S' -> scale <- scale + 5 //5 is arbitrary
            | _ -> ()

    System.Convert.ToDouble(Move(move).Score + scale)


// if an S is used, make sure it is bridging two words (making one plural)
// otherwise set the score to 1. This ensures that the AI will never pass just because
// an S isn't used in two words, but will make the use of single word S's more rare
let SmartSMoves(tiles:TileList, letters: Map<Config.Coordinate, Tile>) = 
    let neighbors(c:Coordinate, o:Orientation) = 
        let n = c.Next(o)
        let p = c.Prev(o)
        [|
            if n.IsValid() then
                yield n
            if p.IsValid() then
                yield p
        |]
    let move = Move(letters)
    let modifiers = letters |> Seq.filter (fun kv -> kv.Value.Letter = 'S') |> Seq.map (fun kv -> 
        let connectors = neighbors(kv.Key, move.Orientation) |> Seq.filter (fun c -> Game.Instance.PlayingBoard.HasTile(c)) |> Seq.length
        if connectors = 0 then
            -5
        else
            0
    )
    let scale = modifiers |> Seq.sum
    System.Convert.ToDouble(move.Score - scale)

let OnlyPlayOver5(tiles:TileList, move: Map<Config.Coordinate, Tile>) = 
    if move.Count <= 5 then 0.0 else System.Convert.ToDouble(Move(move).Score)

let OnlyPlay7s(tiles:TileList, move: Map<Config.Coordinate, Tile>) = 
    if move.Count < 7 then 0.0 else System.Convert.ToDouble(Move(move).Score)

//combine two for the best of both worlds
let SmartSMovesSaveCommon(tiles:TileList, move:Map<Config.Coordinate, Tile>) = 
    max(SmartSMoves(tiles, move), SaveCommon(tiles, move))