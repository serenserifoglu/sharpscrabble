using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core;

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
    }
}