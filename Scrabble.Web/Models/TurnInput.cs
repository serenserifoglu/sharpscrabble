using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scrabble.Web.Models
{
    public class TurnInput
    {
        public String Type { get; set; }
        public TileLocation[] Tiles { get; set; }
    }
}