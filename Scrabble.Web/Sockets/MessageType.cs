using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scrabble.Web.Sockets
{
    public enum MessageType
    {
        Debug,
        DrawTurn,
        GameOver,
        NotifyTurn,
        TilesUpdated
    }
}