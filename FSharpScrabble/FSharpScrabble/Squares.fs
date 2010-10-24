module Scrabble.Core.Squares

[<AbstractClass>]
type Square(letterMult:int, wordMult:int) = 
    let mutable tile = null
    member this.LetterMultiplier with get() = if tile = null then letterMult else 1
    member this.WordMultiplier with get() = if tile = null then wordMult else 1
    member this.Tile with get() = tile and set t = tile <- t
    member this.IsEmpty with get() = tile = null

type NormalSquare() = 
    inherit Square(1, 1)

type DoubleLetterSquare() =
    inherit Square(2, 1)

type TripleLetterSquare() =
    inherit Square(3, 1)

type DoubleWordSquare() =
    inherit Square(1, 2)

type StartSquare() =
    inherit DoubleWordSquare()

type TripleWordSquare() =
    inherit Square(1, 3)