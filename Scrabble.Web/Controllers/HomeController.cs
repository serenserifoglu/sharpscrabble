using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scrabble.Web.Models;
using Scrabble.Core.Types;
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
                GameState state = new GameState(GameVars.DictionaryInstance(), ListModule.OfSeq<Player>(MakePlayers(players)));
                String gameId = new SessionGameLoader().Put(state);
                //state.Start();
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
            foreach (PlayerModel model in players)
                yield return model.ToPlayer(index++);
        }
    }
}
