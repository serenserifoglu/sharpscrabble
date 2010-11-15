using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using Scrabble.Core.Types;

namespace Scrabble.UI
{
    /// <summary>
    /// Kicks off the game and initializes some Windows.
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //want to launch a menu and get some info about players, but for now just start
            StartGame();            
        }

        private void StartGame()
        {
            foreach (HumanPlayer p in Game.Instance.HumanPlayers)
            {
                GameWindow w = new GameWindow(p);
                p.Window = w;
                w.Show();
            }

            //Call this to give each player tiles, and ask the first player for a move.
            //Game.Instance.Start();
        }
                
    }
}
