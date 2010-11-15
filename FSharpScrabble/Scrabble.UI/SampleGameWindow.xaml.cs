using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Scrabble.Core.Types;

namespace Scrabble.UI
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class SampleGameWindow : Window, IGameWindow
    {
        public SampleGameWindow(HumanPlayer p)
        {
            InitializeComponent();

            Player = p;
            this.Title = String.Concat("SharpScrabble - Player: ", p.Name);
        }

        #region Private Methods

        /// <summary>
        /// Some other player, p, has placed some tiles on the board. Show them (or alternatively,
        /// just update the whole board object based on Game.Instance.PlayingBoard.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        private void DrawOpponentTurn(PlaceMove t, Player p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Some other player, p, has dumped some of his or her letters. Not much to do here, 
        /// maybe just log it to a text-status output window thingy.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        private void DrawOpponentTurn(DumpLetters t, Player p)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Opponent p has passed. Not much to do here, just log it to some kind of text status window/control.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        private void DrawOpponentTurn(Pass t, Player p)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IGameWindow Members

        /// <summary>
        /// Some other player has made a move. Show it on the screen.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        public void DrawTurn(Turn t, Player p)
        {
            dynamic dynamicTurn = t;
            DrawOpponentTurn(dynamicTurn, p);
        }

        /// <summary>
        /// It's your turn to act, create a new Turn object and call this.Player.TakeTurn(). Also, you should
        /// update the GUI to reflect this player's actions before calling this.Player.TakeTurn().
        /// </summary>
        public void NotifyTurn()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The player who owns this window
        /// </summary>
        public HumanPlayer Player { get; set; }
        
        /// <summary>
        /// This gets called when the game is finished. The parameter has the winning player(s).
        /// </summary>
        /// <param name="o"></param>
        public void GameOver(GameOutcome o)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
