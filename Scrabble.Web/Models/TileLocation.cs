using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scrabble.Web.Models
{
    public class TileLocation
    {
        public int X { get; set; }
        public int Y { get; set; }
        public char Letter { get; set; }
    }
}