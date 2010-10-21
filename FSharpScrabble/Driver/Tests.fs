module Scrabble.Tests

open Scrabble.Core.Squares
open Scrabble.Core.Types
open Scrabble.Core.Config

let coordTest() = 
    let c0 = Coordinate(8, 0)
    let c1 = Coordinate(8, 5)
    let range = Coordinate.Between(c1, c0)
    range |> Seq.iter (fun c -> c.Print())

let tileTest() = 
    let w = Tile('W')
    printfn "The letter is %c with a score of %i" w.Letter w.Score

let bagTest() = 
    let bag = Bag()
    bag.PrintAll()
    bag.Take(90)
    printfn "I took 90 tiles from the bag."
    bag.PrintRemaining()
    bag.Take(6)
    printfn "I took 6 tiles from the bag."
    bag.PrintRemaining()
    //let's make sure we only get two and don't get an out of range ex ;)
    let lastTiles = bag.Take(7)
    printfn "I wanted to take 7 tiles from the bag, but I only got %i." lastTiles.Length
    printfn "The last tiles I took were:"
    lastTiles |> Seq.iter (fun t -> t.Print())
    printfn "I want to make sure the bag is really empty. This next line should print nothing."
    bag.PrintRemaining()

    //this line will throw
    //bag.Take()

let moveTest() = 
    let m = Move()
    m.AddTile(Coordinate(10, 6), Tile('S'))
    m.AddTile(Coordinate(10, 7), Tile('H'))
    m.AddTile(Coordinate(10, 8), Tile('I'))
    m.AddTile(Coordinate(10, 9), Tile('T'))
    Game.Instance.PlayingBoard.Put(m)

let boardTest() = 
    Game.Instance.PlayingBoard.Put(Tile('W'), Coordinate(0, 0))
    Game.Instance.PlayingBoard.Put(Tile('I'), Coordinate(1, 0))
    Game.Instance.PlayingBoard.Put(Tile('L'), Coordinate(2, 0))
    Game.Instance.PlayingBoard.Put(Tile('L'), Coordinate(3, 0))
    Game.Instance.PlayingBoard.Put(Tile('Y'), Coordinate(5, 3))
    Game.Instance.PlayingBoard.Put(Tile('A'), Coordinate(5, 4))
    Game.Instance.PlayingBoard.Put(Tile('Y'), Coordinate(5, 5))
    Game.Instance.PlayingBoard.PrettyPrint()
