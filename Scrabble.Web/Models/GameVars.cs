using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Scrabble.Core;

namespace Scrabble.Web.Models
{
    public abstract class GameVars
    {
        private static WordLookup.WordLookup dictionary; 

        public static void Initialize()
        {
            dictionary = new WordLookup.WordLookup(HttpContext.Current.Server.MapPath("~/App_Data/twl.txt"));
        }

        public static WordLookup.WordLookup DictionaryInstance()
        {
            return dictionary;
        }
    }
}