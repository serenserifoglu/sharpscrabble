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

        public void DrawTurn(Turn turn, Player player, String summary)
        {
            dynamic dynamicTurn = turn;
            SendTurn(dynamicTurn, player, summary);
        }

        private void SendTurn(Pass turn, Player player, String summary)
        {
            Send(MessageType.DrawTurn, new WrappedPayload("Pass", summary));
        }

        private void SendTurn(DumpLetters turn, Player player, String summary)
        {
            Send(MessageType.DrawTurn, new WrappedPayload("DumpLetters", summary));
        }

        private void SendTurn(PlaceMove turn, Player player, String summary)
        {
            Send(MessageType.DrawTurn, new WrappedPayload("PlaceMove", summary, turn.Letters.ToList()));
        }

        public void GameOver(GameOutcome value)
        {
            Player[] winners = value.Winners.ToArray();
            Send(MessageType.GameOver, winners);
        }

        public void NotifyTurn()
        {
            Send(MessageType.NotifyTurn, null);
        }

        public HumanPlayer Player { get; set; }

        public void TilesUpdated()
        {
            Send(MessageType.TilesUpdated, Player.Tiles);
        }

        private void Send(MessageType type, Object payload)
        {
            Send(new SocketMessage(Player.Id, type, payload).ToJson());
        }

        private void Send(String value)
        {
            String gameId = new SessionGameLoader().CurrentGameId();
            SocketManager.SendMessage(gameId, value);
        }
    }
}