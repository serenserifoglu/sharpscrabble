using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core.Types;

namespace Scrabble.Web.Models
{
    /// <summary>
    /// Copies and holds only the needed fields to be serialized to JSON to send to the browser.
    /// This avoids having to serialize more than just name, score and Id.
    /// </summary>
    public class PlayerSummary
    {
        public PlayerSummary(Player p)
        {
            this.Id = p.Id;
            this.Name = p.Name;
            this.Score = p.Score;
            this.Tiles = p.Tiles.ToArray();
        }

        public int Id { get; private set; }

        public int Score { get; private set; }

        public String Name { get; private set; }

        public Tile[] Tiles { get; private set; }
    }
}