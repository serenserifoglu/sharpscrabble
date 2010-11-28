module Scrabble.Core.Setup

open Scrabble.Core.AI
open Scrabble.Core.HillClimbingAI
open Scrabble.Core.Types
open Scrabble.Core.UtilityFunctions

let SetupComputer() = 
    Game.Instance.ComputerPlayers |> Seq.iter (fun c -> 
        c.Provider <- //HillClimbingMoveGenerator(Game.Instance.Dictionary, 5) //random restart 5 times
                        HillClimbingMoveGenerator(Game.Instance.Dictionary) 
                        //MoveGenerator(Game.Instance.Dictionary)
        c.UtilityFunction <- //MaximumScore
                            //SmartSMoves
                            //SaveCommon
                            OnlyPlay7s
                            //OnlyPlayOver5
    )