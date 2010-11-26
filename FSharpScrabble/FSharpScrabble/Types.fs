namespace Scrabble.Core.Types

open System
open System.Linq;
open System.Collections.Generic
open Scrabble.Core
open Scrabble.Core.Config
open Scrabble.Core.Squares
open Scrabble.Core.Helper
open Scrabble.WordLookup

type Tile(letter:char) = 
    let getScore l = 
        match l with
        | 'E' | 'A' | 'I' | 'O' | 'N'| 'R' | 'T' | 'L' | 'S' | 'U' -> 1
        | 'D' | 'G' -> 2
        | 'B' | 'C' | 'M' | 'P' -> 3
        | 'F' | 'H' | 'V' |  'W' | 'Y' -> 4
        | 'K' -> 5
        | 'J' | 'X' -> 8
        | 'Q' | 'Z' -> 10
        | ' ' -> 0
        | _ -> raise (Exception("Only uppercase characters A - Z and a blank space are supported in Scrabble."))
    let score = getScore letter
    member this.Letter with get() = letter
    member this.Score with get() = score
    member this.Print() = printfn "Letter: %c, Score: %i" this.Letter this.Score
    override this.GetHashCode() =
        this.Letter.GetHashCode()
    override this.Equals(o) =
        match o with
        | :? Tile as other -> this.Letter = other.Letter
        | _ -> false
    //IComparable interface
    interface IComparable with  
         member this.CompareTo(o) = 
            match o with
            | :? Tile as other -> this.Letter.CompareTo(other.Letter)
            | _ -> -1

type TileList = 
    inherit List<Tile>
    new () = { inherit List<Tile>() }
    new (capacity:int) = { inherit List<Tile>(capacity) }
    new (items:IEnumerable<Tile>) = { inherit List<Tile>(items) }
    new (tile:Tile) = { inherit List<Tile>( List.ofArray [| tile |] ) }

    /// This is a dirty hack, but I'm OK with it.
    [<DefaultValue>] val mutable private hash : string

    member this.RemoveMany(l:seq<Tile>) = 
        let remove i = 
            match this.Remove(i) with
            | true -> ()
            | false -> raise (Exception(String.Format("Cannot remove tile '{0}', it is not in the collection.", i.Letter)))
        l |> Seq.iter remove
    member this.Shuffle() = 
        let rng = Random();  
        let mutable n = this.Count  
        while n > 1 do 
            n <- n - 1 
            let k = rng.Next(n + 1)
            let value = this.[k] 
            this.[k] <- this.[n]
            this.[n] <- value
    member this.Score() = 
        this |> Seq.sumBy (fun t -> t.Score)
    member this.Draw(n:int) = 
        let ret = this.Take(n).ToList()
        this.RemoveRange(0, n)
        ret
    member this.TakeChar(c:char) = 
        let tile = Tile(c)
        if this.Remove(tile) then
            tile
        else
            raise (Exception("Tile was not found in the list."))
    member this.HasEqualElements(other:TileList) = 
        if this.Count = other.Count then
            //Well, this is total shit. Sorry functional purists out there, I'm on a deadline.
            let mutable i = 0
            let mutable finished = false
            let mutable result = true
            while i < this.Count && not(finished) do
                if not(this.[i] = other.[i]) then
                    finished <- true
                    result <- false
                i <- i + 1
            result
        else
            false
    member this.PrepareForCompare() = 
        this.Sort()
        this.hash <- this.Select((fun (t:Tile) -> t.Letter.ToString())).Aggregate((fun a b -> String.Concat(a, b))) // wow, F#/BCL interop is a total bitch
    override this.GetHashCode() =
        hash.ToString().GetHashCode()
    override this.Equals(o) =
        match o with
        | :? TileList as other -> this.HasEqualElements(other)                                
        | _ -> false

type Bag() = 
    let mutable pointer = 0;
    let inventory = TileList()
    //populate the bag
    do
        ScrabbleConfig.LetterQuantity |> Seq.iter (fun kv -> nTimes kv.Value (fun () -> inventory.Add(Tile(kv.Key))))
        inventory.Shuffle()

    member this.IsEmpty with get() = inventory.Count = 0
    member this.Print() = inventory |> Seq.iter (fun t -> t.Print())
    member this.Take(n:int) = 
        if this.IsEmpty then
            raise (Exception("The bag is empty, you can not take any tiles."))
        let canTake = System.Math.Min(inventory.Count, n)
        inventory.Draw canTake
    member this.Take() = 
        this.Take(1).[0]
    member this.Put(tiles:seq<Tile>) = 
        inventory.AddRange(tiles)
        inventory.Shuffle()

[<AbstractClass>]
type Turn() =
    abstract member Perform : ITurnImplementor -> unit

and Pass() = 
    inherit Turn()
    override this.Perform(implementor) = 
        implementor.PerformPass()

and DumpLetters(letters:seq<Tile>) = 
    inherit Turn()
    override this.Perform(implementor) = 
        implementor.PerformDumpLetters(this)
    member this.Letters with get() = letters

and PlaceMove(letters:Map<Coordinate, Tile>) =
    inherit Turn()
    override this.Perform(implementor) =
        implementor.PerformMove(this)
    member this.Letters with get() = letters

and ITurnImplementor =
    abstract member PerformPass : unit -> unit
    abstract member PerformDumpLetters : DumpLetters -> unit
    abstract member PerformMove : PlaceMove -> unit
    abstract member TakeTurn : Turn -> unit

type IIntelligenceProvider = 
    abstract member Think : TileList -> Turn

[<AbstractClass>]
type Player(name:string) =
    let tiles = TileList()
    let mutable score = 0
    abstract member NotifyTurn : ITurnImplementor -> unit
    abstract member NotifyGameOver : GameOutcome -> unit
    abstract member DrawTurn : Turn * Player -> unit
    abstract member TilesUpdated : unit -> unit
    member this.Name with get() = name
    member this.Score with get() = score
    member this.Tiles with get() = tiles
    member this.HasTiles with get() = tiles.Count > 0
    member this.AddScore(s) = 
        score <- score + s
    member this.FinalizeScore() =
        score <- score - this.Tiles.Score()
    member this.TakeTurn(implementor:ITurnImplementor, t:Turn) = 
        implementor.TakeTurn(t)

and GameOutcome(winners:seq<Player>) =
    member this.Winners with get() = winners

type ComputerPlayer(name:string, provider:IIntelligenceProvider) = 
    inherit Player(name)
    override this.NotifyTurn(implementor) =
        let turn = provider.Think(this.Tiles)
        this.TakeTurn(implementor, turn)
    override this.NotifyGameOver(_) = 
        () //intentionally left blank
    override this.DrawTurn(_, _) = 
        () //intentionally left blank
    override this.TilesUpdated() = 
        () //intentionally left blank

type HumanPlayer(name:string) =
    inherit Player(name)
    [<DefaultValue>] val mutable private window : IGameWindow
    [<DefaultValue>] val mutable private game : ITurnImplementor
    override this.NotifyTurn(implementor) = 
        this.game <- implementor
        this.window.NotifyTurn()
    override this.NotifyGameOver(o:GameOutcome) = 
        this.window.GameOver(o)
    override this.DrawTurn(t:Turn, p:Player) = 
        this.window.DrawTurn(t, p)
    override this.TilesUpdated() = 
        this.window.TilesUpdated()
    member this.Window with get() = this.window and set w = this.window <- w
    member this.TakeTurn(t:Turn) = 
        base.TakeTurn(this.game, t)

and IGameWindow =
    abstract member NotifyTurn : unit -> unit
    abstract member DrawTurn : Turn * Player -> unit
    abstract member Player : HumanPlayer with get, set
    abstract member GameOver : GameOutcome -> unit
    abstract member TilesUpdated : unit -> unit

type Board() = 
    let grid : Square[,] = Array2D.init ScrabbleConfig.BoardLength ScrabbleConfig.BoardLength (fun x y -> ScrabbleConfig.BoardLayout (Coordinate(x, y))) 
    member this.Get(c:Coordinate) =
        this.Get(c.X, c.Y)
    member this.Get(x:int, y:int) =
        grid.[x, y]
    member this.HasTile(c:Coordinate) = 
        not (this.Get(c).IsEmpty)
    member this.Put(t:Tile, c:Coordinate) = 
        //if not (this.HasTile(c)) then
            this.Get(c).Tile <- t
        //else
          //  raise (Exception("A tile already exists on the square."))
    member this.Put(m:Move) = 
        m.Letters |> Seq.toList |> Seq.iter (fun (pair:Collections.Generic.KeyValuePair<Coordinate, Tile>) -> this.Put(pair.Value, pair.Key))

    member this.OccupiedSquares() : Map<Coordinate, Square> = 
        Map.ofList [ for i in 0 .. (Array2D.length1 grid) - 1 do
                        for j in 0 .. (Array2D.length2 grid) - 1 do
                            let s = Array2D.get grid i j
                            if s.Tile <> null then
                                yield (Coordinate(i, j), s) ]

    member this.HasNeighboringTile(c:Coordinate) =
        c.Neighbors() |> Seq.exists (fun n -> this.HasTile(n))

    member this.PrettyPrint() = 
        printf "   "
        for j in 0 .. (Array2D.length2 grid) - 1 do
            printf "%2i " j
        printfn ""
        for i in 0 .. (Array2D.length1 grid) - 1 do
            printf "%2i " i
            for j in 0 .. (Array2D.length2 grid) - 1 do
                let s = Array2D.get grid j i
                if s.Tile <> null then
                    let tile = s.Tile :?> Tile
                    printf " %c " tile.Letter
                else
                    printf " _ "
            printfn ""

and GameState(players:Player list) = 
    let bag = Bag()
    let board = Board()
    let mutable moveCount = 0
    let rng = Random()
    let mutable currentPlayer = 0 //rng.Next(players.Length)
    let mutable passCount = 0
    let wordLookup = WordLookup()

    //Private functions
    let IsGameComplete() = 
        //a game of Scrabble is over when a player has 0 tiles, or each player has passed twice
        players |> List.exists (fun p -> not p.HasTiles) || passCount = players.Length * 2
    let FinalizeScores() = 
        players |> List.iter (fun p -> p.FinalizeScore())
        let bonus = players |> List.map (fun p -> p.Tiles.Score()) |> List.sum
        players |> List.filter (fun p -> not p.HasTiles) |> List.iter (fun p -> p.AddScore(bonus))
    let WinningPlayers() = 
        //we could have a tie, so this will return a list
        let max = players |> List.maxBy (fun p -> p.Score)
        players |> List.filter (fun p -> p.Score = max.Score) |> Seq.toList
    let FinishGame() =
        FinalizeScores()
        let o = GameOutcome(WinningPlayers())
        players |> List.iter (fun p -> p.NotifyGameOver(o))

    //Interface implementation
    interface ITurnImplementor with
        member this.PerformPass() = 
            passCount <- passCount + 1
        member this.PerformDumpLetters(dl) =
            passCount <- 0
            this.CurrentPlayer.Tiles.RemoveMany(dl.Letters)
            bag.Put(dl.Letters)
            this.GiveTiles(this.CurrentPlayer, dl.Letters.Count())
        member this.PerformMove(turn) =
            passCount <- 0 
            let move = Move(turn.Letters)
            if not move.IsValid then
                raise (InvalidMoveException("Move violates position requirements or forms one or more invalid words."))
            board.Put(move)
            this.CurrentPlayer.AddScore(move.Score)
            this.CurrentPlayer.Tiles.RemoveMany(turn.Letters |> Seq.map (fun kv -> kv.Value))
            this.GiveTiles(this.CurrentPlayer, turn.Letters.Count)
        member this.TakeTurn(t:Turn) =
            t.Perform(this)
            //show this move to the other players
            this.OtherPlayers() |> Seq.iter (fun p -> p.DrawTurn(t, this.CurrentPlayer))
            if IsGameComplete() = false then
                this.NextMove()
            else
                FinishGame()

    //Properties
    member this.TileBag with get() = bag
    member this.PlayingBoard with get() = board
    member this.MoveCount with get() = moveCount and set(x) = moveCount <- x
    member this.IsOpeningMove with get() = moveCount = 0
    member this.Players with get() =  List.toSeq players
    member this.HumanPlayers with get() = this.Players.OfType<HumanPlayer>()
    member this.Dictionary with get() = wordLookup
    member this.CurrentPlayer with get() = List.nth players currentPlayer

    //Private Members
    member private this.NextMove() =
        moveCount <- moveCount + 1
        //increment player
        currentPlayer <- currentPlayer + 1
        if currentPlayer >= players.Length then
            currentPlayer <- 0
        this.CurrentPlayer.NotifyTurn(this)
    member private this.OtherPlayers() = 
        this.OtherPlayers this.CurrentPlayer
    member private this.OtherPlayers(current:Player) = 
        players |> List.filter (fun p -> p <> current)
    member private this.GiveTiles(p:Player, n:int) = 
        let newTiles = bag.Take(n) //if there's less than n tiles in the bag, they get the remaining
        p.Tiles.AddRange(newTiles)
        p.TilesUpdated()

    //Public Members
    member this.Start() = 
        //draw tiles for each player
        players |> List.iter (fun p -> 
            p.Tiles.AddRange(bag.Take ScrabbleConfig.MaxTiles)
            p.TilesUpdated()
        )
        this.CurrentPlayer.NotifyTurn(this)

/// A singleton that will represent the game board, bag of tiles, players, move count, etc.
and Game() = 
    static let instance = lazy(GameState([ ComputerPlayer("Master", BoardScanner()) :> Player; HumanPlayer("Apprentice") :> Player ])) //Pretty sweet, huh? Hard coding stuff...
    //static let instance = lazy(GameState([ HumanPlayer("Apprentice") :> Player; HumanPlayer("Master") :> Player ])) //2 humans, more hard coding
    static member Instance with get() = instance.Value

/// A player's move is a set of coordinates and tiles. This will throw if the move isn't valid.
/// That is, if the tiles aren't layed out properly (not all connected, the word formed doesn't "touch" any other tiles - with the exception of the first word)
/// and if there is a "run" of connected tiles that doesn't form a valid word
and Move(letters:Map<Coordinate, Tile>) = 
    let sorted = letters |> Seq.sortBy ToKey |> Seq.toList
    let first = sorted |> Seq.head |> ToKey
    let last = sorted |> Seq.skip (sorted.Length - 1) |> Seq.head |> ToKey
    let CheckBoardPrev(c:Coordinate, o:Orientation) = 
        let prev = c.Prev(o)
        prev.IsValid() && Game.Instance.PlayingBoard.HasTile(c)
    let CheckBoardNext(c:Coordinate, o:Orientation) = 
        let next = c.Next(o)
        next.IsValid() && Game.Instance.PlayingBoard.HasTile(c)
    let range = 
        try
            Coordinate.Between(first, last)
        with 
            | UnsupportedCoordinateException(msg) -> raise (InvalidMoveException(msg))
    let orientation = 
        if letters.Count = 1 then
            //need to do some special checking if the player only played a single tile
            if CheckBoardNext(first, Orientation.Vertical) || CheckBoardPrev(first, Orientation.Vertical) then
                Orientation.Vertical
            else
                Orientation.Horizontal
        else if first.X = last.X then
            Orientation.Vertical
        else
            Orientation.Horizontal

    //Private methods
    let NotOverwritingTiles() = 
        not(letters |> Seq.map (fun kv -> kv.Key) |> Seq.exists (fun key -> Game.Instance.PlayingBoard.HasTile(key)))
    let CheckMoveOccupied(c:Coordinate) =
            letters.ContainsKey(c) || Game.Instance.PlayingBoard.HasTile(c)
    let Opposite(o:Orientation) =
        match o with
        | Orientation.Horizontal -> Orientation.Vertical
        | _ -> Orientation.Horizontal
    let IsAligned() = 
        if letters.Count <= 1 then
            true
        else
            let c0 = (Seq.head letters) |> ToKey // note: added the helper method "ToKey" to replace this: (fun pair -> pair.Key)
            let v = letters |> Seq.map (fun pair -> pair.Key.X) |> Seq.forall (fun x -> c0.X = x)
            let h = letters |> Seq.map (fun pair -> pair.Key.Y) |> Seq.forall (fun y -> c0.Y = y)
            v || h
    let IsConsecutive() =
        range |> Seq.forall (fun c -> CheckMoveOccupied(c))
    let IsConnected() = 
        range |> Seq.exists (fun c -> Game.Instance.PlayingBoard.HasTile(c) || Game.Instance.PlayingBoard.HasNeighboringTile(c))
    let ContainsStartSquare() = 
        letters.ContainsKey(ScrabbleConfig.StartCoordinate)
    let ValidPlacement() = 
        NotOverwritingTiles() && IsAligned() && IsConsecutive() && ((Game.Instance.IsOpeningMove && ContainsStartSquare()) || (not Game.Instance.IsOpeningMove && IsConnected()))
    let ComputeRuns() : Run list = 
        let alt = Opposite(orientation)
        let alternateRuns = sorted |> Seq.map (fun pair -> Run(pair.Key, alt, letters)) |> Seq.filter (fun r -> r.Length > 1) |> Seq.toList
        Run(first, orientation, letters) :: alternateRuns
    let ValidRuns(runs: Run list) = 
        runs |> Seq.forall (fun r -> r.IsValid())
    let ComputeScore(runs : Run list) =
        let score = runs |> List.sumBy (fun r -> r.Score())
        if letters.Count = ScrabbleConfig.MaxTiles then
            score + ScrabbleConfig.AllTilesBonus
        else
            score
    let score = 
        if ValidPlacement() then
            //make sure every sequence of tiles with length > 1 formed by this move is a valid word
            let runs = ComputeRuns()
            if ValidRuns(runs) then
                ComputeScore(runs)
            else
                -1 //raise (InvalidMoveException("One or more invalid words were formed by this move."))
        else
            -1 //raise (InvalidMoveException("Move violates positioning rules (i.e. not connected to other tiles)."))
    
    let valid = score >= 0

    member this.Orientation with get() = orientation
    member this.Letters with get() = letters
    member this.Score with get() = score
    member this.IsValid with get() = valid
    

/// A Run is a series of connected letters in a given direction. This type takes a location and direction and constructs a map of connected tiles to letters in the given direction.
and Run(c:Coordinate, o:Orientation, moveLetters:Map<Coordinate, Tile>) = 
    let GetTileFromMove(c:Coordinate) = 
        match moveLetters.TryFind c with
        | Some t -> t :> obj
        | None -> Game.Instance.PlayingBoard.Get(c).Tile
    let rec Check(c:Coordinate, o:Orientation, increment) =
        if not (c.IsValid()) then
            []
        else 
            let s = Game.Instance.PlayingBoard.Get(c)
            let t = GetTileFromMove(c)
            if t <> null then
                let next = increment(c, o)
                (s, t) :: Check(next, o, increment)
            else
                []
            
    let prevSquares = Check(c, o, (fun (c:Coordinate, o:Orientation) -> c.Prev(o)))
    let nextSquares = Check(c.Next(o), o, (fun (c:Coordinate, o:Orientation) -> c.Next(o)))
    let squares = (List.rev prevSquares) @ nextSquares 

    member this.Orientation with get() = o
    member this.Squares with get() = squares
    member this.Length with get() = squares.Length
    member this.ToWord() =
        squares |> List.map (fun (s, t) -> t :?> Tile) |> List.map (fun t -> t.Letter.ToString()) |> List.reduce (fun s0 s1 -> s0 + s1)
    member this.IsValid() = 
        Game.Instance.Dictionary.IsValidWord(this.ToWord())
    member this.Score() =
        let wordMult = squares |> List.map (fun (s, t) -> s.WordMultiplier) |> List.reduce (fun a b -> a * b)
        let letterScore = squares |> List.map (fun (s, t) -> (s, t :?> Tile)) |> List.map (fun (s, t) ->  s.LetterMultiplier * t.Score ) |> List.sum
        wordMult * letterScore

and BoardScanner() = 
    let hNext(c:Coordinate) = 
        c.Next(Orientation.Horizontal)
    let vNext(c:Coordinate) =
        c.Next(Orientation.Vertical)
    ///the time complexity of this is going to be O(terrible)
    member private this.Scan(tileCount:int, minorIncrement, majorIncrement) = 
        let coordScan(start:Coordinate, tileCount:int, nextFn) = 
            if Game.Instance.PlayingBoard.HasTile(start) then
                []
            else
                let mutable i = tileCount - 1
                let mutable finish = start
                let mutable stop = false
                while i > 0 && not(stop) do
                    if not(Game.Instance.PlayingBoard.HasTile(finish)) then
                        i <- i - 1
                    let next : Coordinate = nextFn finish
                    if next.IsValid() then
                        finish <- nextFn finish
                    else
                        stop <- true

                let last = finish
                let positions = [ for c in Coordinate.Between(start, last) do
                                    if not(Game.Instance.PlayingBoard.HasTile(c)) then
                                        let pos = CandidatePosition(start, c)
                                        if pos.IsValid then
                                            yield pos ]
                positions
            
        let rec localScan(start:Coordinate, tileCount:int, nextFn) =
            if not(start.IsValid()) then
                []
            else
                let next = nextFn start
                coordScan(start, tileCount, nextFn) @ localScan(next, tileCount, nextFn)

        let rec fullScan(start:Coordinate, tileCount:int, minorIncrement, majorIncrement) =
            if not(start.IsValid()) then
                []
            else
                let next = majorIncrement start
                localScan(start, tileCount, minorIncrement) @ fullScan(next, tileCount, minorIncrement, majorIncrement)

        fullScan(Coordinate(0, 0), tileCount, minorIncrement, majorIncrement)

    member private this.Make(word:string, tiles:IEnumerable<Tile>, pos:CandidatePosition) = 
        let upperWord = word.ToUpper()
        let localTiles = TileList(tiles)
        let length = pos.Coordinates |> Array.length
        let layout = Map.ofList [ for i = 0 to length - 1 do
                                    let tile = localTiles.TakeChar(upperWord.Chars(i))
                                    let coord = pos.Coordinates.[i] :> Coordinate
                                    yield (coord, tile) ]
        Move(layout)
    member private this.Make(tiles:Tile list, pos:CandidatePosition) = 
        let emptyCoords = pos.EmptyCoordinates() |> Seq.toArray;
        let letters = Map.ofList [  for i = 0 to tiles.Length - 1 do
                                        let coord = emptyCoords.[i] :> Coordinate
                                        yield (coord, tiles.[i]) ]
        Move(letters)
    member private this.FirstTurn(tiles:TileList) = 
        let letters = tiles |> Seq.map (fun t -> t.Letter) |> Seq.toList
        let choices = Game.Instance.Dictionary.FindAllWords(letters)
        //imperitive style because I'm bad...
        let moves = [
            for word in choices do
                for pos in this.FirstMovePositions(word.Length) do
                    yield this.Make(word, tiles, pos)
        ]
        let move = moves |> Seq.maxBy (fun m -> m.Score)
        PlaceMove(move.Letters) :> Turn
    member private this.FirstMovePositions(length:int) = 
        //this is so sloppy, this goes on the refactor/shit list. I just need to get going with something that works...
        seq{
            let center = ScrabbleConfig.StartCoordinate
            if length < 6 then
                yield CandidatePosition(center, Coordinate(center.X, center.Y + length - 1))
            else if length = 6 then
                yield CandidatePosition(center, Coordinate(center.X, center.Y + length - 1))
                yield CandidatePosition(Coordinate(center.X, center.Y - 1), Coordinate(center.X, center.Y + length - 2))
            else
                yield CandidatePosition(center, Coordinate(center.X, center.Y + length - 1))
                yield CandidatePosition(Coordinate(center.X, center.Y - 1), Coordinate(center.X, center.Y + length - 2))
                yield CandidatePosition(Coordinate(center.X, center.Y - 2), Coordinate(center.X, center.Y + length - 3))
        }
    member private this.MidGame(tiles:TileList) = 
        (*
            Ok here's the stupidest brute force way of doing this. This will totally suck and be really slow. But this is just my first attempt.
            - Take the tiles, and compute the power set
            - Remove the duplicates from the power set
            - Call Scan() to get all possible valid positions
            - For each possible position, take all the sets of tiles of equal length from the power set
                - Then, for each of those sets, permute it and create a Move() for each permutation, and add the Move() to a list
            - Take the max of the resulting Move list
        *)
        let powerset = subsets (tiles |> Seq.toList)
        let ofLength(powerset:Tile list list, l:int) = 
            powerset |> List.filter (fun list -> list.Length = l)

        let rows = tiles.Count + 1
        let index : Tile list list array = Array.zeroCreate rows

        for i = 0 to tiles.Count do
            index.[i] <- ofLength(powerset, i)
        
        let moves = List<Move>()

        let candidates = this.Scan(tiles.Count, hNext, vNext) @ this.Scan(tiles.Count, vNext, hNext)
        for candidate in candidates do
            let length = candidate.EmptyCoordinates() |> Seq.length
            let sets = index.[length]
            for s in sets do
                for p in permute s do
                    let move = this.Make(p, candidate)
                    if move.IsValid then
                        moves.Add(move)
        
        if moves.Count > 0 then
            let best = moves.OrderByDescending((fun (m:Move) -> m.Score)).First()
            PlaceMove(best.Letters) :> Turn 
        else
            Pass() :> Turn
    interface IIntelligenceProvider with
        member this.Think(tiles) =
            if Game.Instance.IsOpeningMove then
                this.FirstTurn(tiles)
            else
                this.MidGame(tiles)

and CandidatePosition(start:Coordinate, finish:Coordinate) = 
    let coords = Coordinate.Between(start, finish)
    let valid = coords |> Seq.exists (fun c -> Game.Instance.PlayingBoard.HasTile(c) || Game.Instance.PlayingBoard.HasNeighboringTile(c))
    member this.Coordinates with get() = coords
    member this.Start with get() = start
    member this.Finish with get() = finish
    member this.IsValid with get() = valid
    member this.EmptyCoordinates() = 
        coords |> Seq.filter (fun c -> not (Game.Instance.PlayingBoard.HasTile(c)))
    member this.PermuteWith(tiles:seq<Tile>) = 
        raise (NotImplementedException())
    