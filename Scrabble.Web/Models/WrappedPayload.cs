using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scrabble.Web.Models
{
    public class WrappedPayload
    {
        public WrappedPayload(String what, String summary, int newScore)
        {
            What = what;
            Summary = summary;
            NewScore = newScore;
        }

        public WrappedPayload(String what, String summary, int newScore, Object payload)
        {
            What = what;
            Summary = summary;
            NewScore = newScore;
            Payload = payload;
        }

        public String What { get; private set; }
        public String Summary { get; private set; }
        public int NewScore { get; private set; }
        public Object Payload { get; private set; }
    }
}