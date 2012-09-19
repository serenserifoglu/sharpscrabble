using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Scrabble.Web.Models;
using Scrabble.Web.Sockets;
using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Config;
using SuperSocket.SocketEngine;
using SuperSocket.SocketEngine.Configuration;
using SuperWebSocket;

namespace Scrabble.Web
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute(
                "Play",
                "play/{id}/{action}",
                new { controller = "Play", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //Setup the web socket server
            StartSuperWebSocketByConfig();

            //Setup some core config stuff, like where we persist games
            Scrabble.Core.Types.Game.Loader = new SessionGameLoader();
            Scrabble.Web.Models.GameVars.Initialize();
        }

        private void StartSuperWebSocketByConfig()
        {
            var serverConfig = ConfigurationManager.GetSection("socketServer") as SocketServiceConfig;
            if (!SocketServerManager.Initialize(serverConfig))
                return;

            var socketServer = SocketServerManager.GetServerByName("SuperWebSocket") as WebSocketServer;

            Application["WebSocketPort"] = socketServer.Config.Port;

            socketServer.NewMessageReceived += new SessionEventHandler<WebSocketSession, string>(socketServer_NewMessageReceived);
            socketServer.NewSessionConnected += new SessionEventHandler<WebSocketSession>(socketServer_NewSessionConnected);
            socketServer.SessionClosed += new SessionEventHandler<WebSocketSession, CloseReason>(socketServer_SessionClosed);

            if (!SocketServerManager.Start())
                SocketServerManager.Stop();
        }

        void socketServer_NewMessageReceived(WebSocketSession session, string e)
        {
            SocketManager.HandleMessage(session, e);
        }

        void socketServer_NewSessionConnected(WebSocketSession session)
        {
            SocketManager.AddSession(session);
        }

        void socketServer_SessionClosed(WebSocketSession session, CloseReason reason)
        {
            SocketManager.RemoveSession(session, reason);
        }
    }
}