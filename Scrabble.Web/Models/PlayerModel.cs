using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core;
using Scrabble.Core.Types;
using Microsoft.FSharp.Core;

namespace Scrabble.Web.Models
{
    public class PlayerModel
    {
        public String Name { get; set; }

        public PlayerType Type { get; set; }

        /// <summary>
        /// Only for computer players.
        /// </summary>
        public String Provider { get; set; }

        /// <summary>
        /// Only for computer players.
        /// </summary>
        public String UtilityFunction { get; set; }

        public bool IsValid()
        {
            if (Type == PlayerType.Human)
                return !String.IsNullOrWhiteSpace(Name);
            else
                return !String.IsNullOrWhiteSpace(Name) && NonNegativeInt(Provider) && NonNegativeInt(UtilityFunction);
        }

        private bool NonNegativeInt(String s)
        {
            int value;
            return Int32.TryParse(s, out value) && value >= 0;
        }

        public Player ToPlayer(int index)
        {
            if (Type == PlayerType.Human)
                return new HumanPlayer(Name, index);
            else
            {
                ComputerPlayer com = new ComputerPlayer(Name, index);
                Setup.ApplySetupValues(GameVars.DictionaryInstance(), com, Provider, UtilityFunction);
                return com;
            }
        }
    }
}