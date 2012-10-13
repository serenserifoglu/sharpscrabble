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
        public WebGameWindow(Player p)
        {
            this.Player = p;
        }

        public void DrawTurn(Turn turn, Player who, String summary)
        {
            dynamic dynamicTurn = turn;
            SendTurn(dynamicTurn, who, summary);
        }

        protected virtual void SendTurn(Pass turn, Player who, String summary)
        {
            Send(who.Id, MessageType.DrawTurn, new WrappedPayload("Pass", summary, who.Score));
        }

        protected virtual void SendTurn(DumpLetters turn, Player who, String summary)
        {
            Send(who.Id, MessageType.DrawTurn, new WrappedPayload("DumpLetters", summary, who.Score));
        }

        protected virtual void SendTurn(PlaceMove turn, Player who, String summary)
        {
            Send(who.Id, MessageType.DrawTurn, new WrappedPayload("PlaceMove", summary, who.Score, turn.Letters.ToList()));
        }

        public virtual void GameOver(GameOutcome value)
        {
            Player[] winners = value.Winners.ToArray();
            Send(MessageType.GameOver, winners);
        }

        public void NotifyTurn()
        {
            Send(MessageType.NotifyTurn, null);
        }

        public Player Player { get; set; }

        public void TilesUpdated()
        {
            Send(MessageType.TilesUpdated, Player.Tiles);
        }

        protected void Send(MessageType type, Object payload)
        {
            Send(Player.Id, type, payload);
        }

        protected void Send(int who, MessageType type, Object payload)
        {
            Send(new SocketMessage(who, type, payload).ToJson());
        }

        protected void Send(String value)
        {
            String gameId = new SessionGameLoader().CurrentGameId();
            SocketManager.SendMessage(gameId, value);
        }
    }
}