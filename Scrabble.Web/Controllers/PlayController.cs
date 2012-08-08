using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scrabble.Core.Types;

namespace Scrabble.Web.Controllers
{
    public class PlayController : Controller
    {
        public ActionResult Index(String id)
        {
            Board b = Game.Instance.PlayingBoard;
            foreach (KeyValuePair<Core.Config.Coordinate, Core.Squares.Square> pair in b.OccupiedSquares())
            {

            }
            return View("Index", (object)id);
        }

    }
}
