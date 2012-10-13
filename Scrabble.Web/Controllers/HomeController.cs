using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scrabble.Web.Models;
using Scrabble.Core;
using Scrabble.Core.Types;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;

namespace Scrabble.Web.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Setup(ICollection<PlayerModel> players)
        {
            if (Validate(players))
            {
                //Convert the form data to actual Player instances
                GameState state = new GameState(GameVars.DictionaryInstance(), ListModule.OfSeq<Player>(MakePlayers(players)));

                //Persist the game, and let humans know about the GameId
                String gameId = new SessionGameLoader().Put(state);

                //Also set the GameId as a cookie, so we can identify the web socket used later.
                HttpCookie cookie = new HttpCookie("GameId", gameId);
                Response.Cookies.Add(cookie);
                return RedirectToAction("Index", "Play", new { id = gameId });
            }
            else
                return RedirectToAction("Index");
        }

        private bool Validate(ICollection<PlayerModel> players)
        {
            foreach (PlayerModel model in players)
            {
                if (!model.IsValid())
                    return false;
            }
            return true;
        }

        private IEnumerable<Player> MakePlayers(ICollection<PlayerModel> players)
        {
            int index = 0;
            bool hasHuman = players.Any(model => model.Type == PlayerType.Human);
            bool mainGameWindowAdded = false;

            foreach (PlayerModel model in players)
            {
                Player p = model.ToPlayer(index++, hasHuman, mainGameWindowAdded);
                if (p is ComputerPlayer)
                {
                    ComputerPlayer com = (ComputerPlayer)p;
                    mainGameWindowAdded = mainGameWindowAdded || com.Window != null;
                }
                else if (p is HumanPlayer)
                    mainGameWindowAdded = true;

                yield return p;
            }
        }
    }
}
