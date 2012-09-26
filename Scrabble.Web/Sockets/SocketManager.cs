using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SuperWebSocket;
using SuperSocket.SocketBase;

namespace Scrabble.Web.Sockets
{
    /// <summary>
    /// Used to maintain browsers that are connected via web sockets, and the game Id to which they belong.
    /// </summary>
    public class SocketManager
    {
        private static Dictionary<String, WebSocketSession> sessions = new Dictionary<String, WebSocketSession>();
        private static object sessionLock = new object();

        public static void AddSession(WebSocketSession session)
        {
            lock (sessionLock)
            {
                String gameId = GameIdFromSession(session);
                if (String.IsNullOrWhiteSpace(gameId))
                    throw new Exception("No GameId found in cookie.");
                sessions.Add(gameId, session);

                SendMessage(gameId, "Got new session");
            }
        }

        public static void RemoveSession(WebSocketSession session, CloseReason reason)
        {
            lock (sessionLock)
            {
                String gameId = GameIdFromSession(session);
                if (!String.IsNullOrWhiteSpace(gameId))
                    sessions.Remove(gameId);
            }
        }

        public static void HandleMessage(WebSocketSession session, String value)
        {

        }

        public static void SendMessage(String gameId, String value)
        {
            if (gameId == null)
                throw new ArgumentNullException("gameId must be non-null.");
            if (value == null)
                throw new ArgumentNullException("value must be non-null.");

            lock (sessionLock)
            {
                if (sessions.ContainsKey(gameId))
                    sessions[gameId].SendResponseAsync(value);
            }
        }

        private static String GameIdFromSession(WebSocketSession session)
        {
            return session.Cookies["GameId"];
        }
    }
}