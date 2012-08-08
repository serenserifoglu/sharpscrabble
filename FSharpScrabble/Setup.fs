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