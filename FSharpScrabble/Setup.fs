module Scrabble.Core.Setup

open System
open Scrabble.Core.AI
open Scrabble.Core.HillClimbingAI
open Scrabble.Core.Types
open Scrabble.Core.UtilityFunctions
open Scrabble.WordLookup

let ApplySetupValues(wordLookup:WordLookup, player:ComputerPlayer, providerCode, utilityCode) = 
    player.Provider <-
        match providerCode with
        | "0" -> MoveGenerator(wordLookup) :> IIntelligenceProvider
        | "1" -> HillClimbingMoveGenerator(wordLookup, 15) :> IIntelligenceProvider
        | "2" -> HillClimbingMoveGenerator(wordLookup, 5) :> IIntelligenceProvider  
        | _ -> raise (Exception("Unknown provider code."))
    player.UtilityFunction <-
        match utilityCode with
        | "0" -> MaximumScore
        | "1" -> SmartSMoves
        | "2" -> SaveCommon
        | "3" -> OnlyPlay7s
        | "4" -> OnlyPlayOver5
        | "5" -> UseBonusSquares
        | _ -> raise (Exception("Unknown utility function code."))
    () 

(*let SetupGameState() = 
    Game.Instance <- GameState([ 
                                ComputerPlayer("PlayerOne") :> Player 
                                ;ComputerPlayer("PlayerTwo") :> Player 
                               ])


let SetupComputer() = 
    Game.Instance.ComputerPlayers |> Seq.iter (fun c -> 
        c.Provider <- //HillClimbingMoveGenerator(Game.Instance.Dictionary, 5) //random restart 5 times
                        //HillClimbingMoveGenerator(Game.Instance.Dictionary) 
                        MoveGenerator(Game.Instance.Dictionary)
        c.UtilityFunction <- MaximumScore
                            //SmartSMoves
                            //SaveCommon
                            //OnlyPlay7s
                            //OnlyPlayOver5
                            //UseBonusSquares
    )

let SetupFirstComputer() = 
    let first = Game.Instance.ComputerPlayers |> Seq.head
    
    first.Provider <- //HillClimbingMoveGenerator(Game.Instance.Dictionary, 5) //random restart 5 times
                        //HillClimbingMoveGenerator(Game.Instance.Dictionary) 
                        MoveGenerator(Game.Instance.Dictionary)
    first.UtilityFunction <- MaximumScore
                            //SmartSMoves
                            //SaveCommon
                            //OnlyPlay7s
                            //OnlyPlayOver5
                            //UseBonusSquares

let SetupSecondComputer() = 
    let second = Game.Instance.ComputerPlayers |> Seq.toList |> List.tail |> List.head
    
    second.Provider <- //HillClimbingMoveGenerator(Game.Instance.Dictionary, 15) //random restart 5 times
                        //HillClimbingMoveGenerator(Game.Instance.Dictionary) 
                        MoveGenerator(Game.Instance.Dictionary)
    second.UtilityFunction <- MaximumScore
                            //SmartSMoves
                            //SaveCommon
                            //OnlyPlay7s
                            //OnlyPlayOver5
                            //UseBonusSquares *)