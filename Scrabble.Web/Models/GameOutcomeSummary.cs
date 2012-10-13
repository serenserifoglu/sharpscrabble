using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core.Types;

namespace Scrabble.Web.Models
{
    /// <summary>
    /// Class used to limit the amount of serialized info to JSON.
    /// </summary>
    public class GameOutcomeSummary
    {
        public GameOutcomeSummary(GameOutcome outcome)
        {
            Winners = outcome.Winners.Select(p => new PlayerSummary(p)).ToArray();
            AllPlayers = outcome.AllPlayers.Select(p => new PlayerSummary(p)).ToArray();
        }

        public PlayerSummary[] Winners { get; private set; }
        public PlayerSummary[] AllPlayers { get; private set; }
    }
}