using System.Collections.Generic;
using Scrabble.Core;

namespace Scrabble.Dictionary
{
    public interface IWordLookup
    {
        List<string> FindAllWords (IEnumerable<Types.Tile> letters, int minLength = 2, int maxLength = 15);
        bool IsValidWord(string word);
    }
}
