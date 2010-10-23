module Scrabble.Tests

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

let MoveTest() = 
    let m = Move(Map.ofList [(Coordinate(10, 6), Tile('S')); (Coordinate(10, 7), Tile('H')); (Coordinate(10, 8), Tile('I')); (Coordinate(10, 9), Tile('T'))])    
    let aligned = m.IsAligned()
    let consecutive = m.IsConsecutive()
    
    Game.Instance.PlayingBoard.Put(m)
    Game.Instance.PlayingBoard.PrettyPrint()


let ValidConnectedMoves() = 
    let m0 = Move(Map.ofList [ (Coordinate(10, 6), Tile('H')); (Coordinate(10, 7), Tile('E')); (Coordinate(10, 8), Tile('L')); (Coordinate(10, 9), Tile('L')); (Coordinate(10, 10), Tile('O'))])

    let aligned = m0.IsAligned()
    let consecutive = m0.IsConsecutive()
    let connected = m0.IsConnected()
    
    Game.Instance.PlayingBoard.Put(m0)
    Game.Instance.NextMove()

    let m1 = Move(Map.ofList [ (Coordinate(9, 10), Tile('W')); (Coordinate(11, 10), Tile('R')); (Coordinate(12, 10), Tile('L')); (Coordinate(13, 10), Tile('D'))])

    let aligned = m1.IsAligned()
    let consecutive = m1.IsConsecutive()
    let connected = m1.IsConnected()

    Game.Instance.PlayingBoard.Put(m1)

    Game.Instance.PlayingBoard.PrettyPrint()

let DisjointMoves() = 
    let m0 = Move(Map.ofList [ (Coordinate(10, 6), Tile('H')); (Coordinate(10, 7), Tile('E')); (Coordinate(10, 8), Tile('L')); (Coordinate(10, 9), Tile('L')); (Coordinate(10, 10), Tile('O'))])

    let aligned = m0.IsAligned()
    let consecutive = m0.IsConsecutive()
    let connected = m0.IsConnected()
    
    Game.Instance.PlayingBoard.Put(m0)
    Game.Instance.NextMove()

    let m1 = Move(Map.ofList[ (Coordinate(5, 12), Tile('P')); (Coordinate(6, 12), Tile('O')); (Coordinate(7, 12), Tile('O')); (Coordinate(8, 12), Tile('P'))])

    let aligned = m1.IsAligned()
    let consecutive = m1.IsConsecutive()
    let connected = m1.IsConnected()

    Game.Instance.PlayingBoard.Put(m1)

    Game.Instance.PlayingBoard.PrettyPrint()

let ExtendMove() = 
    let m0 = Move(Map.ofList [ (Coordinate(10, 6), Tile('H')); (Coordinate(10, 7), Tile('E')); (Coordinate(10, 8), Tile('L')); (Coordinate(10, 9), Tile('L')); ])

    let aligned = m0.IsAligned()
    let consecutive = m0.IsConsecutive()
    let connected = m0.IsConnected()
    
    Game.Instance.PlayingBoard.Put(m0)
    Game.Instance.NextMove()

    let m1 = Move(Map.ofList[ (Coordinate(9, 10), Tile('P')); (Coordinate(10, 10), Tile('O')); (Coordinate(11, 10), Tile('O')); (Coordinate(12, 10), Tile('P')) ])

    let aligned = m1.IsAligned()
    let consecutive = m1.IsConsecutive()
    let connected = m1.IsConnected()

    Game.Instance.PlayingBoard.Put(m1)

    Game.Instance.PlayingBoard.PrettyPrint()

let RunTest() = 
    let m = Move(Map.ofList [ (Coordinate(10, 6), Tile('B')); (Coordinate(10, 7), Tile('A')); (Coordinate(10, 8), Tile('N')); (Coordinate(10, 9), Tile('A')); (Coordinate(10, 10), Tile('N')); (Coordinate(10, 11), Tile('A'));])
    Game.Instance.PlayingBoard.Put(m)
    let m2 = Move(Map.ofList [ (Coordinate(10, 12), Tile('S')); (Coordinate(11, 12), Tile('H')); (Coordinate(12, 12), Tile('I')); (Coordinate(13, 12), Tile('T')) ] )
    Game.Instance.PlayingBoard.Put(m2)

    let r = Run(Coordinate(10, 8), Orientation.Vertical)
    let banana = r.ToWord()
    Game.Instance.PlayingBoard.PrettyPrint()

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

    let tiles = seq [| Tile('E'); Tile('A'); Tile('T'); Tile('S'); Tile('T');  |]

    let watch = System.Diagnostics.Stopwatch()
    watch.Start()
    let words = lookup.FindAllWords(tiles)
    watch.Stop()

    words |> Seq.iter (printf "%s\n")
    printfn "word lookup time: %im %is %ims" watch.Elapsed.Minutes watch.Elapsed.Seconds watch.Elapsed.Milliseconds
