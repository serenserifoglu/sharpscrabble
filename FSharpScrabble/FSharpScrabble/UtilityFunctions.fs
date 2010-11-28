module Scrabble.Core.UtilityFunctions

open Scrabble.Core.Types

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
let SmartSMoves(tiles:TileList, move: Map<Config.Coordinate, Tile>) = 
    let mutable scale = 1
    for item in move do
        if item.Value.Letter = 'S' then 
            //one item already touching it, and this move isn't just the S, or more than one item touching it
            let mutable bordering = 0
            for tile in item.Key.Neighbors() do
                if Game.Instance.PlayingBoard.HasTile(tile) then 
                    bordering <- bordering + 1
            if (bordering = 1 && move.Count > 1) || bordering > 1 then 
                scale <- scale + 10 // 10 is arbitrary
            else                
                scale <- (0 - Move(move).Score) + 1 //this will make the return value 1
    System.Convert.ToDouble(Move(move).Score + scale)


let OnlyPlayOver5(tiles:TileList, move: Map<Config.Coordinate, Tile>) = 
    if move.Count <= 5 then 0.0 else System.Convert.ToDouble(Move(move).Score)

let OnlyPlay7s(tiles:TileList, move: Map<Config.Coordinate, Tile>) = 
    if move.Count < 7 then 0.0 else System.Convert.ToDouble(Move(move).Score)

//combine two for the best of both worlds
let SmartSMovesSaveCommon(tiles:TileList, move:Map<Config.Coordinate, Tile>) = 
    max(SmartSMoves(tiles, move), SaveCommon(tiles, move))