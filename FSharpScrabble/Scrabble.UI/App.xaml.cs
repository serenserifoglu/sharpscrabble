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
            foreach (HumanPlayer p in Game.Instance.HumanPlayers)
            {
                SampleGameWindow w = new SampleGameWindow(p);
                p.Window = w;
                w.Show();
            }
        }
    }
}
