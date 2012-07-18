namespace Scrabble.Core.Squares

[<AbstractClass>]
type Square(letterMult:int, wordMult:int, cssClass:string) = 
    let mutable tile = null
    member this.LetterMultiplier with get() = if tile = null then letterMult else 1
    member this.WordMultiplier with get() = if tile = null then wordMult else 1
    member this.Tile with get() = tile and set t = tile <- t
    member this.IsEmpty with get() = tile = null
    member this.CssClass with get() = cssClass

type NormalSquare() = 
    inherit Square(1, 1, "")

type DoubleLetterSquare() =
    inherit Square(2, 1, "doubleLetter")

type TripleLetterSquare() =
    inherit Square(3, 1, "tripleLeter")

type DoubleWordSquare() =
    inherit Square(1, 2, "doubleWord")

type StartSquare() =
    inherit Square(1, 2, "start")

type TripleWordSquare() =
    inherit Square(1, 3, "tripleWord")