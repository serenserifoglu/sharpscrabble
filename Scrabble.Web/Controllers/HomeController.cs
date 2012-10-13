using System;
using System.Configuration;
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
                return NewGame(MakePlayers(players));
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult QuickStart(String quickName)
        {
            if (String.IsNullOrWhiteSpace(quickName))
                quickName = ConfigurationManager.AppSettings["DefaultHumanName"];

            return NewGame(MakeQuickStartPlayers(quickName));
        }

        public ActionResult SampleAI()
        {
            return NewGame(MakeSamplePlayers());
        }

        private ActionResult NewGame(IEnumerable<Player> players)
        {
            //Convert the form data to actual Player instances
            GameState state = new GameState(GameVars.DictionaryInstance(), ListModule.OfSeq<Player>(players));

            //Persist the game, and let humans know about the GameId
            String gameId = new SessionGameLoader().Put(state);

            //Also set the GameId as a cookie, so we can identify the web socket used later.
            HttpCookie cookie = new HttpCookie("GameId", gameId);
            Response.Cookies.Add(cookie);
            return RedirectToAction("Index", "Play", new { id = gameId });
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

        private IEnumerable<Player> MakeQuickStartPlayers(String humanName)
        {
            HumanPlayer human = new HumanPlayer(humanName, 0);
            human.Window = new WebGameWindow(human);
            yield return human;

            ComputerPlayer com = new ComputerPlayer(ConfigurationManager.AppSettings["DefaultComputerName0"], 1);
            Scrabble.Core.Setup.ApplySetupValues(GameVars.DictionaryInstance(), com, "0", "0");
            yield return com;
        }

        private IEnumerable<Player> MakeSamplePlayers()
        {
            ComputerPlayer com = new ComputerPlayer(ConfigurationManager.AppSettings["DefaultComputerName0"], 0);
            com.Window = new WebGameWindow(com);
            Scrabble.Core.Setup.ApplySetupValues(GameVars.DictionaryInstance(), com, "0", "0");
            yield return com;

            ComputerPlayer com1 = new ComputerPlayer(ConfigurationManager.AppSettings["DefaultComputerName1"], 1);
            com1.Window = new SecondaryWebGameWindow(com1);
            Scrabble.Core.Setup.ApplySetupValues(GameVars.DictionaryInstance(), com1, "0", "0");
            yield return com1;
        }
    }
}
