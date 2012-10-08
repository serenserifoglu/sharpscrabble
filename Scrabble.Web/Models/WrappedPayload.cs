using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scrabble.Web.Models
{
    public class WrappedPayload
    {
        public WrappedPayload(String what, String summary)
        {
            What = what;
            Summary = summary;
        }

        public WrappedPayload(String what, String summary, Object payload)
        {
            What = what;
            Summary = summary;
            Payload = payload;
        }

        public String What { get; private set; }
        public String Summary { get; private set; }
        public Object Payload { get; private set; }
    }
}