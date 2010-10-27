module Scrabble.Tests

open Scrabble.Core
open Scrabble.Core.Squares
open Scrabble.Core.Types
open Scrabble.Core.Config
open Scrabble.Dictionary

let CoordTest() = 
    let c0 = Coordinate(8, 0)
    let c1 = Coordinate(8, 5)
    let range = Coordinate.Between(c1, c0)
    range |> Seq.iter (fun c -> c.Print())

let TileTest() = 
    let w = Tile('W')
    printfn "The letter is %c with a score of %i" w.Letter w.Score

let BagTest() = 
    let bag = Bag()
    bag.PrintAll()
    let whoCares = bag.Take(90)
    printfn "I took 90 tiles from the bag."
    bag.PrintRemaining()
    let whoCares = bag.Take(6)
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

let MoveTest() = 
    let m = Move(Map.ofList [(Coordinate(7, 7), Tile('S')); (Coordinate(7, 8), Tile('H')); (Coordinate(7, 9), Tile('I')); (Coordinate(7, 10), Tile('T'))])    
    printfn "Move score: %i" m.Score  
    Game.Instance.PlayingBoard.Put(m)
    Game.Instance.PlayingBoard.PrettyPrint()

let MoveTest2() = 
    let m = Move(Map.ofList [ (Coordinate(5, 7), Tile('S')); (Coordinate(6, 7), Tile('T')); (Coordinate(7, 7), Tile('A')); (Coordinate(8, 7), Tile('N')); (Coordinate(9, 7), Tile('D')) ])
    printfn "first move score = %i" m.Score
    Game.Instance.PlayingBoard.Put(m)
    Game.Instance.NextMove()
    let m2 = Move(Map.ofList [ (Coordinate(9, 8), Tile('O')); (Coordinate(10, 8), Tile('V')); (Coordinate(11, 8), Tile('E')); (Coordinate(12, 8), Tile('N')); ])
    printfn "second move score = %i" m2.Score
    Game.Instance.PlayingBoard.Put(m2)
    Game.Instance.PlayingBoard.PrettyPrint()

let InvalidMove() = 
    let m = Move(Map.ofList [ (Coordinate(5, 7), Tile('S')); (Coordinate(6, 7), Tile('T')); (Coordinate(7, 7), Tile('A')); (Coordinate(8, 7), Tile('N')); (Coordinate(9, 7), Tile('D')) ])
    printfn "first move score = %i" m.Score
    Game.Instance.PlayingBoard.Put(m)
    Game.Instance.NextMove()
    try
        let m2 = Move(Map.ofList [ (Coordinate(8, 8), Tile('O')); (Coordinate(9, 8), Tile('V')); (Coordinate(10, 8), Tile('E')); (Coordinate(11, 8), Tile('N')); ])
        Game.Instance.PlayingBoard.Put(m2)
    with
        | InvalidMoveException(msg) -> printfn "Invalid move: %s" msg
let ValidWordTest() =
    let valid1 = Game.Instance.Dictionary.IsValidWord("banana")
    let valid2 = Game.Instance.Dictionary.IsValidWord("piss")
    let valid3 = Game.Instance.Dictionary.IsValidWord("noob")
    ()

let BoardTest() = 
    Game.Instance.PlayingBoard.Put(Tile('W'), Coordinate(0, 0))
    Game.Instance.PlayingBoard.Put(Tile('I'), Coordinate(1, 0))
    Game.Instance.PlayingBoard.Put(Tile('L'), Coordinate(2, 0))
    Game.Instance.PlayingBoard.Put(Tile('L'), Coordinate(3, 0))
    Game.Instance.PlayingBoard.Put(Tile('Y'), Coordinate(5, 3))
    Game.Instance.PlayingBoard.Put(Tile('A'), Coordinate(5, 4))
    Game.Instance.PlayingBoard.Put(Tile('Y'), Coordinate(5, 5))
    Game.Instance.PlayingBoard.PrettyPrint()

let PrintNeighbors(x:int, y:int) = 
    let c0 = Coordinate(x, y)
    c0.Neighbors() |> Seq.iter (fun c -> c.Print())

let NeighborTest() = 
    printfn "Neighboring squares of (0, 0) are:"
    PrintNeighbors(0, 0)
    printfn "Neighboring squares of (4, 6) are:"
    PrintNeighbors(4, 6)
    printfn "Neighboring squares of (14, 10) are:"
    PrintNeighbors(14, 10)



let DictionaryTest() =
    //this will take a few seconds to initialize the dictionary's data structure
    let lookup = WordLookup() 

    let tiles = seq [| 'E'; 'A'; 'T'; 'S'; 'T';  |]

    let watch = System.Diagnostics.Stopwatch()
    watch.Start()
    let words = lookup.FindAllWords(tiles)
    watch.Stop()

    words |> Seq.iter (printf "%s\n")
    printfn "word lookup time: %im %is %ims" watch.Elapsed.Minutes watch.Elapsed.Seconds watch.Elapsed.Milliseconds
