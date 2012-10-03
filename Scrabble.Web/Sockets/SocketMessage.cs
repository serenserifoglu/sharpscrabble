using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace Scrabble.Web.Sockets
{
    public class SocketMessage
    {
        public SocketMessage(int playerId, MessageType t, Object payload)
        {
            PlayerId = playerId;
            MessageType = t;
            Payload = payload;
        }

        [ScriptIgnore]
        public MessageType MessageType { get; private set; }

        public int PlayerId { get; private set; }

        public String What { get { return MessageType.ToString(); } }

        public Object Payload { get; private set; }

        public String ToJson()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}