module Scrabble.Core.UtilityFunctions

open Scrabble.Core.Types


let MaximumScore(tiles:TileList, move) = System.Convert.ToDouble(Move(move).Score)

let SaveCommon(tiles:TileList, move) = 0.0
    
let SmartSMoves(tiles:TileList, move) = 0.0
