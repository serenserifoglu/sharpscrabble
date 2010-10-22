using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Scrabble.Core;

namespace Scrabble.Dictionary
{
    public class WordLookup : IWordLookup
    {
        //this could really be a list, but a dictionary provides faster lookups, and this is large..
        // The key is the word with letters sorted in alphabetical order. This removes the need to permute
        // the strings, since we can sort and lookup instead.
        private static Dictionary<string, List<string>> OfficialWords;

        static WordLookup ()
        {
            OfficialWords = new Dictionary<string, List<string>>();

            var words = File.ReadAllLines(@"..\..\..\Dictionary\OfficialDictionary\twl.txt")
                .ToList()
                .ConvertAll(x => x.ToLower());

            foreach (var word in words)
            {
                var alphabetized = new string(word.OrderBy(_ => _).ToArray());

                List<string> existing = null;
                if (OfficialWords.TryGetValue(alphabetized, out existing))
                    existing.Add(word);
                else
                    OfficialWords.Add(alphabetized, new List<string> { word });
            }
        }

        /// <summary>
        /// Finds all possible words for the given input tiles. Loops from i = minLength to min(maxLength, letters.Length),
        /// finding all possible ways to choose i letters from the input set.  Each combination is sorted alphabetically 
        /// and matched against the official word dictionary (this saves computation of n! permutations).
        /// A list of words is returned, all scoring must be done by the caller.
        /// </summary>
        /// <param name="letters">Tiles to be used, both in hand and on board</param>
        /// <param name="minLength">Length of the shortest possible words that should be returned (default 2).</param>
        /// <param name="maxLength">Length of the longest possible words that should be returned (default 15, or the number of tiles input)</param>
        /// <returns></returns>
        public List<string> FindAllWords (IEnumerable<Types.Tile> letters, int minLength = 2, int maxLength = 15)
        {
            var length = letters.Count();
            if(length < maxLength)
                maxLength = length;

            var chars = letters.Select(x => x.Letter).ToArray();
            List<string> validWords = new List<string>();

            for (int i = minLength; i <= maxLength; i++)
            {
                var generator = new CombinationGenerator(length, i);
                // for each combination of length i
                while(generator.HasNext)
                {
                    var builder = new StringBuilder();
                    var indices = generator.GetNext();

                    // get the combination, put it into string form
                    for (int j = 0; j < indices.Length; j++)
                        builder.Append(chars[indices[j]]);

                    var possible = new string(builder.ToString().OrderBy(_ => _).ToArray()).ToLower();

                    List<string> values;
                    if (OfficialWords.TryGetValue(possible, out values))
                        foreach (var item in values)
                            if (!validWords.Contains(item)) //avoid duplicates
                                validWords.Add(item);
                }
            }

            return validWords;
        }
    }
}
