using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scrabble.Dictionary
{
    /// <summary>
    /// Generator for all r-length combinations from n elements.
    /// E.g. CombinationGenerator(4,2) = {1,2,3,4} choose 2 = {1,2}, {1,3}, {1,4}, {2,3}, {2,4}, {3,4}.
    /// Order does not matter, {1,2} == {2,1}.
    /// 
    /// Evaluated semi-lazily, generates one combination at a time by calls to GetNext()
    /// in case we want to short-circuit at all for better performance.
    /// 
    /// Returns int[] as set representation - use as indexes into list/array for combinations of other types/objects.
    /// </summary>
    public class CombinationGenerator
    {
        public long TotalCombinations { get; private set; } // n! / (r! * (n-r)!), for n choose r
        public bool HasNext { get { return _combinationsLeft != 0; } }

        private static readonly Dictionary<int, long> _factorials = new Dictionary<int, long>();

        private long _combinationsLeft;
        private int[] _array;
        private int _domain; //total items in the domain, top number in choose syntax
        private int _choose; //number of items chosen from the domain, bottom number in choose syntax

        /// <summary>
        /// We only need 0 to 15, it's expensive each time, and never changes...
        /// </summary>
        static CombinationGenerator ()
        {
            PreLoadFactorials();
        }

        public CombinationGenerator (int domain, int choose)
        {
            if (domain < 1 || choose > domain)
                throw new ApplicationException("Arguments violate the constraints of the combine operator");

            _domain = domain;
            _choose = choose;
            
            ComputeTotalCombinations();
            _combinationsLeft = TotalCombinations;

            _array = new int[choose];
            for (int i = 0; i < _array.Length; i++)
            {
                _array[i] = i;
            }
        }

        public int[] GetNext ()
        {
            // first time in, take the initial configuration (1..r)
            if (_combinationsLeft == TotalCombinations)
            {
                _combinationsLeft--;
                return _array;
            }

            var i = _choose - 1;
            
            while (_array[i] == _domain - _choose + i)
            {
                i--;
            }

            _array[i] = _array[i] + 1;
            for (int j = i + 1; j < _choose; j++)
            {
                _array[j] = _array[i] + j - i;
            }

            _combinationsLeft--;
            return _array;
        }

        private void ComputeTotalCombinations ()
        {
            // compute total # of combinations (= n! / (r! * (n-r)!))
            TotalCombinations = _factorials[_domain] / (_factorials[_choose] * _factorials[_domain - _choose]);
        }

        private static void PreLoadFactorials()
        {
            _factorials.Add(0, 1);
            _factorials.Add(1, 1);
            _factorials.Add(2, 2);
            _factorials.Add(3, 6);
            _factorials.Add(4, 24);
            _factorials.Add(5, 120);
            _factorials.Add(6, 720);
            _factorials.Add(7, 5040);
            _factorials.Add(8, 40320);
            _factorials.Add(9, 362880);
            _factorials.Add(10, 3628800);
            _factorials.Add(11, 39916800);
            _factorials.Add(12, 479001600);
            _factorials.Add(13, 6227020800);
            _factorials.Add(14, 87178291200);
            _factorials.Add(15, 1307674368000); //1,307,674,368,000 = 1.3E12. This could take some time.
        }
    }
}
