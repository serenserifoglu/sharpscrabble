//A dumping ground for random functions
module Scrabble.Core.Helper

open System

let ToKey (pair : Collections.Generic.KeyValuePair<_, _>) =
    pair.Key

let ToValue (pair : Collections.Generic.KeyValuePair<_, _>) =
    pair.Value