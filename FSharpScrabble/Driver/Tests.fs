module Scrabble.Tests

open System
open Scrabble.Core.AI
open Scrabble.Core
open Scrabble.Core.Squares
open Scrabble.Core.Types
open Scrabble.Core.Config
open Scrabble.WordLookup

let CoordTest() = 
    let c0 = Coordinate(8, 0)
    let c1 = Coordinate(8, 5)
    let range = Coordinate.Between(c1, c0)
    range |> Seq.iter (fun c -> c.Print())

let TileTest() = 
    let w = Tile('W')
    printfn "The letter is %c with a score of %i" w.Letter w.Score

let TileListTest() = 
    let t = TileList()
    printfn "The score of an empty list is %i" (t.Score())
    
let AllTileTest() = 
    printfn "Randomized tiles:"
    let t = TileList()
    ScrabbleConfig.LetterQuantity |> Seq.iter (fun kv -> Helper.nTimes kv.Value (fun () -> t.Add(Tile(kv.Key))))
    t.Shuffle()
    t |> Seq.iter (fun t -> t.Print())

let BagTest() = 
    let bag = Bag()
    bag.Print()
    let lotsOfTiles = bag.Take(90)
    printfn "I took 90 tiles from the bag."
    bag.Print()
    let whoCares = bag.Take(6)
    printfn "I took 6 tiles from the bag."
    bag.Print()
    //let's make sure we only get two and don't get an out of range ex ;)
    let lastTiles = bag.Take(7)
    printfn "I wanted to take 7 tiles from the bag, but I only got %i." lastTiles.Count
    printfn "The last tiles I took were:"
    lastTiles |> Seq.iter (fun t -> t.Print())
    printfn "I want to make sure the bag is really empty. This next line should print nothing."
    bag.Print()

    try
        let whatever = bag.Take()
        ()
    with
        | ex -> printfn "Taking from an empty bag yielded the following exception: %s" (ex.Message)

let MoveTest() = 
    let m = Move(Map.ofList [(Coordinate(7, 7), Tile('S')); (Coordinate(7, 8), Tile('H')); (Coordinate(7, 9), Tile('I')); (Coordinate(7, 10), Tile('T'))])    
    printfn "Move score: %i" m.Score  
    Game.Instance.PlayingBoard.Put(m)
    Game.Instance.PlayingBoard.PrettyPrint()

(* - These next two tests don't compile anymore
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
*)

let ValidWordTest() =
    let valid1 = Game.Instance.Dictionary.IsValidWord("banana")
    let valid2 = Game.Instance.Dictionary.IsValidWord("piss")
    let valid3 = Game.Instance.Dictionary.IsValidWord("noob")
    ()

let OverwriteWordTest() = 
    let firstLetters = Map.ofList [ (Coordinate(5, 7), Tile('S')); (Coordinate(6, 7), Tile('T')); (Coordinate(7, 7), Tile('A')); (Coordinate(8, 7), Tile('N')); (Coordinate(9, 7), Tile('D')) ]
    let secondLetters = Map.ofList [ (Coordinate(5, 7), Tile('S')); (Coordinate(5, 8), Tile('T')); (Coordinate(5, 9), Tile('A')); (Coordinate(5, 10), Tile('R')); ]
    let thirdLetters = Map.ofList [ (Coordinate(5, 8), Tile('T')); (Coordinate(5, 9), Tile('A')); (Coordinate(5, 10), Tile('R')); ]
    
    let openingMove = Move(firstLetters)
    printfn "Move.ToString() = %s" (openingMove.ToString())
    Game.Instance.PlayingBoard.Put(openingMove)
    printfn "First move score: %i" openingMove.Score
    Game.Instance.PlayingBoard.PrettyPrint()
    Game.Instance.MoveCount <- Game.Instance.MoveCount + 1
    let invalidMove = Move(secondLetters)
    printfn "Is second move valid? %b" invalidMove.IsValid
    let validMove = Move(thirdLetters)
    printfn "Is third move valid? %b" validMove.IsValid
    if validMove.IsValid then
        Game.Instance.PlayingBoard.Put(validMove)
        printfn "Third move score: %i" validMove.Score
    Game.Instance.PlayingBoard.PrettyPrint()
    //play STANDING
    let fourthLetters = Map.ofList [ (Coordinate(10, 7), Tile('I')); (Coordinate(11, 7), Tile('N')); (Coordinate(12, 7), Tile('G')); ]
    let anotherValidWord = Move(fourthLetters)
    printfn "Is fourth move valid? %b" anotherValidWord.IsValid
    if anotherValidWord.IsValid then
        Game.Instance.PlayingBoard.Put(anotherValidWord)
        printfn "Fourth move score: %i" anotherValidWord.Score
    Game.Instance.PlayingBoard.PrettyPrint()
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
    let lookup = new WordLookup() 

    let tiles = seq [| 'C';'R';'N';'O';'E';'R';'L';  |] |> Seq.toList

    let watch = System.Diagnostics.Stopwatch()
    watch.Start()
    let words = lookup.FindAllWords(tiles)
    watch.Stop()

    words |> Seq.iter (printf "%s\n")
    printfn "word lookup time: %im %is %ims" watch.Elapsed.Minutes watch.Elapsed.Seconds watch.Elapsed.Milliseconds


let AIFirstMoveTest() = 
    let gen = new MoveGenerator(new WordLookup())
    let move = gen.Think(TileList [ new Tile('R'); new Tile('E'); new Tile('I'); new Tile('F'); new Tile('T'); new Tile('C'); new Tile('A'); ], (fun t -> Convert.ToDouble(t)))
    printf "word: "
    (move :?> PlaceMove).Letters |> Seq.iter (fun w -> printf "%c" w.Value.Letter) 
    //printfn " "
    //printfn "score: %i" (move :?> PlaceMove).Score


let AIMultiMoveTest() = 
    let gen = new MoveGenerator(new WordLookup())

    let watch = System.Diagnostics.Stopwatch()
    watch.Start()

    for i in 0 .. 10 do
        let move = gen.Think(TileList(Game.Instance.TileBag.Take(7)), UtilityFunctions.MaximumScore)
        
        match move.GetType().ToString() with
            | "Scrabble.Core.Types.Pass" -> printfn "Pass"
            | _ -> 
                    printf "word: "
                    (move :?> PlaceMove).Letters |> Seq.iter (fun w -> printf "%c" w.Value.Letter) 
                    printfn " "

                    for i in (move :?> PlaceMove).Letters do
                        Game.Instance.PlayingBoard.Put(i.Value, i.Key)

                    Game.Instance.PlayingBoard.PrettyPrint() |> ignore
                    Game.Instance.MoveCount <- Game.Instance.MoveCount + 1

    watch.Stop()
    printfn "AI elapsed time: %im %is %ims" watch.Elapsed.Minutes watch.Elapsed.Seconds watch.Elapsed.Milliseconds
    System.Console.Read()

    
