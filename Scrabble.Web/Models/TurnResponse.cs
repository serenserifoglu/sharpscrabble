using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Scrabble.Web.Models
{
    public class TurnResponse
    {
        public TurnResponse(bool valid, String summary)
        {
            IsValid = valid;
            Summary = summary;
        }

        public bool IsValid { get; private set; }
        public String Summary { get; private set; }
    }
}