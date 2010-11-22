module Scrabble.Driver

open Scrabble.Core.Squares
open Scrabble.Core.Types
open Scrabble.Core.Config
open Scrabble.Core.Helper

open Scrabble.Tests

(* 
  This will serve as a dumping ground for invoking random tests for now, 
  but eventually we can invoke our GUI here, whether it's WPF or something else.
*)

//CoordTest()
//BoardTest()
//NeighborTest()
//DictionaryTest()
//ValidWordTest()

//MoveTest2()

//TileListTest()
//AllTileTest()

//BagTest()

let b = Bag()
let tiles = TileList(7)
tiles.AddRange(b.Take(7))
tiles.PrepareForCompare()

let powerset = subsets (tiles |> Seq.toList)
let ofLength(powerset:Tile list list, l:int) = 
    powerset |> List.filter (fun list -> list.Length = l)

let index : Tile list list array = Array.zeroCreate tiles.Count

for i = 0 to tiles.Count - 1 do
    index.[i] <- ofLength(powerset, i)


let l : Tile list = List.empty
let p = permute l

printfn "asdf"