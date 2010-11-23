module Scrabble.Core.AI

open Scrabble.Core.Config
open Scrabble.Core.Squares
open Scrabble.Core.Types
open Scrabble.WordLookup

type MoveGenerator(lookup:WordLookup) =

    // first word of the game must include (7,7)
    let PossibleStarts(word:string, o:Orientation): Coordinate list =
        let highestStart:int = max 0 (7 - word.Length + 1)

        [for i in highestStart .. 7 do
            match o with
                Orientation.Vertical -> yield new Coordinate(7, i)
                | _ -> yield new Coordinate(i, 7)]

    // simple case - this is the first move, so there is nothing on the board.
    // we just choose the highest scoring word and find the best place to play it
    // as long as the center tile is included
    let CalculateFirstMove(tilesInHand:seq<Tile>): Move = 
        let possibleWords = lookup.FindAllWords(tilesInHand |> Seq.map (fun w -> w.Letter) |> Seq.toList)
        let orientations:seq<Orientation> = Seq.cast(System.Enum.GetValues(typeof<Orientation>))

        let moves = 
            [| for o in orientations do
                   for word in possibleWords do
                       for start in PossibleStarts(word, o) do
                           yield Move(
                                Map.ofSeq 
                                    [| for i in 0 .. word.Length-1 do
                                          yield (start.Next(o, i), new Tile(word.ToUpper().[i]))
                                    |])
            |]

        moves |> Seq.maxBy (fun t -> t.Score) 



    // returns a list of starting coordinates that would result in a valid play of the given
    // word at the given tile for the given orientation.
    // By getting here, there must be at least one way to play the word at the tile, but it
    // may not be valid given the current tiles on the board
    let ValidMoves(c:Coordinate, word:string, o:Orientation, b:Board): Move list = 
        //first generate all possible
        let letter = (b.Get(c).Tile :?> Tile).Letter
        
        // these are valid on a clear board 
        // (if there are no tiles in the way and if on invalid words are created as a side effect)
        let uncheckedStarts = 
            [| for i in 0 .. word.Length - 1 do
                if word.ToUpper().[i] = letter then
                    match o with
                        | Orientation.Horizontal -> if (c.X - i + 1) >= 0 then yield Coordinate(c.X - i + 1, c.Y)
                        | _ -> if (c.Y - i + 1) >= 0 then yield Coordinate(c.X, c.Y - i + 1) |]
        
        [for start in uncheckedStarts do
            let move = Move(Map.ofSeq 
                                [| for i in 0 .. word.Length-1 do
                                        yield (start.Next(o, i), new Tile(word.ToUpper().[i]))
                                |])
            if move.IsValid then yield move]

    //for each occupied square, find all possible words using that tile and the letters in hand.
    // then find all ways to play on that tile, and check if each is valid.
    // from all valid moves, take the max score
    let CalculateBestMove(tilesInHand:seq<Tile>, b:Board): Move = 
        let letters = tilesInHand |> Seq.map (fun w -> w.Letter) |> Seq.toList
        let orientations:seq<Orientation> = Seq.cast(System.Enum.GetValues(typeof<Orientation>))

        let moves = 
            [| for coordinate in b.OccupiedSquares() do
                let tile = b.Get(coordinate.Key).Tile :?> Tile
                let possibleWords = lookup.FindWordsUsing(tile.Letter :: letters, 0)

                for orient in orientations do
                    for word in possibleWords do
                        for move in ValidMoves(coordinate.Key, word, orient, b) do
                            yield move
            |]  
        
        moves |> Seq.maxBy (fun t -> t.Score)



    /// doesn't care if this is the first move or any subsequent move
    /// returns the 'best' move as defined by the move wit the highest score based on the 
    /// Move type's Score method.  We can pass in a Score method (takes Move, returns score) 
    /// to eval against in the future to change strategies
    member this.DetermineBestMove (tilesInHand:seq<Tile>, b:Board): Move = 
        match b.OccupiedSquares().IsEmpty with
                true -> CalculateFirstMove(tilesInHand)
                | false -> CalculateBestMove(tilesInHand, b)