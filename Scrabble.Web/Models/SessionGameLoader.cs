using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Scrabble.Core.Types;

namespace Scrabble.Web.Models
{
    public class SessionGameLoader : IGameLoader
    {
        private static string KEY = "GAME_KEY";

        public GameState Load()
        {
            object o = HttpContext.Current.Session[KEY];
            if (o is GameState)
                return (GameState)o;

            throw new Exception("Game not in Session");
        }

        /// <summary>
        /// Stores a GameState, and returns a unique identifier.
        /// </summary>
        /// <param name="state">The GameState to store.</param>
        /// <returns></returns>
        public String Put(GameState state)
        {
            HttpContext.Current.Session[KEY] = state;
            String id = GenerateId();
            //validate when the DB gets set up
            return id;
        }

        private string GenerateId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        } 
    }
}
