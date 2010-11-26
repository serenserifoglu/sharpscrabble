module Scrabble.Core.UtilityFunctions

open Scrabble.Core.Types


let MaximumScore(tiles, move) = System.Convert.ToDouble(Move(move).Score)

let SaveCommon(tiles, move) = 0.0
    
let SmartSMoves(tiles, move) = 0.0
