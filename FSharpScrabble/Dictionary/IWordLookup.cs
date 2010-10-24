using System.Collections.Generic;
using Scrabble.Core.Types;

namespace Scrabble.Dictionary
{
    public interface IWordLookup
    {
        List<string> FindAllWords (IEnumerable<Tile> letters, int minLength = 2, int maxLength = 15);
        bool IsValidWord(string word);
    }
}
