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
                sessions.Add(GameIdFromSession(session), session);
            }
        }

        public static void RemoveSession(WebSocketSession session, CloseReason reason)
        {
            
        }

        public static void HandleMessage(WebSocketSession session, String value)
        {

        }

        private static String GameIdFromSession(WebSocketSession session)
        {
            return session.Cookies["GameId"];
        }
    }
}