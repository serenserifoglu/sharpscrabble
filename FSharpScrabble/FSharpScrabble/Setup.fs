module Scrabble.Core.Setup

open Scrabble.Core.AI
open Scrabble.Core.Types
open Scrabble.Core.UtilityFunctions

let SetupComputer() = 
    Game.Instance.ComputerPlayers |> Seq.iter (fun c -> 
        c.Provider <- MoveGenerator(Game.Instance.Dictionary)
        c.UtilityFunction <- MaximumScore
    )