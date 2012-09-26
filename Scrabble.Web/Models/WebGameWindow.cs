using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core.Types;
using Scrabble.Web.Sockets;

namespace Scrabble.Web.Models
{
    public class WebGameWindow : IGameWindow
    {
        public WebGameWindow(HumanPlayer p)
        {
            this.Player = p;
        }

        public void DrawTurn(Turn turn, Player player)
        {
            Send("DrawTurn");
        }

        public void GameOver(GameOutcome value)
        {
            Send("GameOver");
        }

        public void NotifyTurn()
        {
            Send("NotifyTurn");
        }

        public HumanPlayer Player { get; set; }


        public void TilesUpdated()
        {
            Send("TilesUpdated");
        }

        private void Send(String value)
        {
            String gameId = new SessionGameLoader().CurrentGameId();
            SocketManager.SendMessage(gameId, value);
        }
    }
}