using System.Collections.Generic;

namespace Scrabble.Dictionary
{
    public interface IWordLookup
    {
        List<string> FindAllWords (IEnumerable<char> letters, int minLength = 2, int maxLength = 15);
        bool IsValidWord(string word);
    }
}
