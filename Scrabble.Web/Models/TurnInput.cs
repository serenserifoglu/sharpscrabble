using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core.Types;

namespace Scrabble.Web.Models
{
    public class TurnInput
    {
        public String Type { get; set; }
        public TileLocation[] Tiles { get; set; }

        public Turn ToTurn(Player current)
        {
            if (Type == "Pass")
                return new Pass();
            else if (Type == "DumpLetters")
                return new DumpLetters(current.Tiles);
            else if (Type == "PlaceMove")
            {
                List<Tuple<Core.Config.Coordinate, Core.Types.Tile>> tupleList =
                    new List<Tuple<Core.Config.Coordinate, Core.Types.Tile>>();

                foreach (TileLocation tileLocation in Tiles)
                {
                    tupleList.Add(
                        new Tuple<Core.Config.Coordinate, Core.Types.Tile>(
                            new Core.Config.Coordinate(tileLocation.X, tileLocation.Y),
                            new Core.Types.Tile(tileLocation.Letter))
                        );
                }

                Microsoft.FSharp.Collections.FSharpMap<Core.Config.Coordinate, Core.Types.Tile> map =
                    new Microsoft.FSharp.Collections.FSharpMap<Core.Config.Coordinate, Core.Types.Tile>(tupleList);
                PlaceMove pm = new PlaceMove(map);
                return pm;
            }
            else
                throw new Exception("Invalid TurnInput Type.");
        }
    }
}