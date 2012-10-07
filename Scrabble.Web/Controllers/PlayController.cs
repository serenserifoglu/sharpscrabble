using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scrabble.Web.Models;
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
                //Do something lol
            }
            GameState state =  new SessionGameLoader().Load();
            return View(new PlayModel(id, state));
        }

        public ActionResult Continue(String id)
        {
            GameState state = new SessionGameLoader().Load();
            if (state == null)
                throw new HttpException(400, "Game state not found");
            state.Continue();
            return Content("true");
        }

        public ActionResult TakeTurn(String id, TurnInput input)
        {
            return Json(new TurnResponse(true, "not yet implemented"));
        }
    }
}
