namespace Scrabble.WordLookup

open System
open System.IO
open System.Text
open System.Collections.Generic
open Scrabble.Dictionary

type WordLookup() = 
    static let ValidWords = new HashSet<string>()
    static let mutable OfficialWords:Map<string, string list> = Map.empty

    static do 
        File.ReadAllLines(@"..\..\..\Dictionary\OfficialDictionary\twl.txt")
            |> Seq.map(fun w -> w.ToLower())
            |> Seq.iter(fun w -> 
                            ValidWords.Add w |> ignore
                            let alphabetized = new string (w |> Seq.sort |> Seq.toArray)

                            match OfficialWords.TryFind alphabetized with
                                // this is terrible, but you can't modify an existing member in the map
                                //pretty sure there has to be a functional way to build this map but it's evading me...
                                | Some x -> OfficialWords <- OfficialWords.Remove(alphabetized); 
                                            OfficialWords <- OfficialWords.Add(alphabetized, w :: x)
                                | None -> OfficialWords <- OfficialWords.Add(alphabetized, w::[])
                        )

    
    member this.IsValidWord word = if String.IsNullOrEmpty word then false else ValidWords.Contains (word.ToLower())

    member this.FindAllWords (letters, ?minLength, ?maxLength) = 
        let minLen = defaultArg minLength 2
        let maxLen = defaultArg maxLength 15

        this.Find(letters, minLen, maxLen)

    member this.FindWordsUsing (letters, useCharAt, ?minLength, ?maxLength) = 
        let minLen = defaultArg minLength 2
        let maxLen = defaultArg maxLength 15

        this.Find(letters, minLen, maxLen, useCharAt)

    member this.Find (letters:char list, minLength:int, maxLength:int, ?useCharAt:int) = 
        let useChar = defaultArg useCharAt -1
        let length = Seq.length letters
        let max = min length maxLength

        let chars = match useChar with
                        | -1 -> letters |> Seq.toArray
                        | _ -> letters |> Seq.filter(fun l -> l <> letters.[useChar]) |> Seq.toArray

        let validWords =
            seq{ for i in minLength .. max do
                     let generator = new CombinationGenerator(length, i)
                     while generator.HasNext do
                         let indices = generator.GetNext()
                         let word = new string([| for j in 0 .. indices.Length - 1 do 
                                                    yield chars.[indices.[j]] 
                                                  match useChar with
                                                      | -1 -> ()
                                                      | _ -> yield chars.[useChar] |])
                         let possible = new string(word |> Seq.sort |> Seq.toArray)

                         match OfficialWords.TryFind(possible.ToLower()) with
                             | None -> ()
                             | Some x -> for item in x do yield item
               }

        match useChar with
            | -1 -> validWords |> Seq.distinct |> Seq.toList
            | _ -> match OfficialWords.TryFind(letters.[useChar].ToString()) with
                    | None -> validWords |> Seq.distinct |> Seq.toList
                    | Some x -> x @ (validWords |> Seq.distinct |> Seq.toList)