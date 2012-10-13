using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core.Types;

namespace Scrabble.Web.Models
{
    /// <summary>
    /// For a game with no human player, the first ComputerPlayer will get an instance of WebGameWindow,
    /// and all others will get an instance of SecondaryWebGameWindow.
    /// 
    /// The main game window will draw all turns, messages, and the game outcome, whereas the secondary game
    /// windows will show just tile updates for the other players.
    /// </summary>
    public class SecondaryWebGameWindow : WebGameWindow
    {
        public SecondaryWebGameWindow(Player p)
            : base(p)
        {

        }

        public override void GameOver(GameOutcome value)
        {
            //Do nothing, the main Game window will show the result.
        }

        protected override void SendTurn(DumpLetters turn, Player who, string summary)
        {
            //Do nothing, the main Game window will show the turn.
        }

        protected override void SendTurn(Pass turn, Player who, string summary)
        {
            //Do nothing, the main Game window will show the turn.
        }

        protected override void SendTurn(PlaceMove turn, Player who, string summary)
        {
            //Do nothing, the main Game window will show the turn.
        }
    }
}