using System.Collections.Generic;
using System.Windows;
using Scrabble.Core;
using Scrabble.Core.Types;
using System.Linq;

namespace Scrabble.UI
{
    /// <summary>
    /// Kicks off the game and initializes some Windows.
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            PlayerWindows = new LinkedList<GameWindow>();
            //want to launch a menu and get some info about players, but for now just start
            StartGame();            
        }

        private void StartGame()
        {
            Setup.SetupGameState();

            //show player windows
            foreach (HumanPlayer p in Game.Instance.HumanPlayers)
            {
                GameWindow w = new GameWindow(p);
                p.Window = w;
                w.Show();
                if (PlayerWindows.Count > 0)
                    PlayerWindows.AddAfter(PlayerWindows.Last, w);
                else
                    PlayerWindows.AddFirst(w);
            }
            
            if (Game.Instance.HumanPlayers.Count() < 1)
            {
                foreach (ComputerPlayer cp in Game.Instance.ComputerPlayers)
                {
                    AIWindow w = new AIWindow(cp);
                    cp.Window = w;
                    w.Show();
                }
            }

            //Give each ComputerPlayer an AI Provider instance and utility functions
            Setup.SetupComputer();

            //Call this to give each player tiles, and ask the first player for a move.
            Game.Instance.Start();
                        
        }

        //To support more than 2 people - this might already be handled in the f#
        public LinkedList<GameWindow> PlayerWindows { get; set; }
                
    }
}
