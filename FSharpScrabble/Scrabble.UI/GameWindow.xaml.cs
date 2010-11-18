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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Scrabble.Core.Types;

namespace Scrabble.UI
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window, IGameWindow
    {
        //Each player gets one of these, they "own" it
        public GameWindow(HumanPlayer p)
        {
            InitializeComponent();

            Player = p;
            PlayerTiles.PlayerName = p.Name;
            this.Title = String.Concat("SharpScrabble - Player: ", p.Name);
            WordInPlay = new Dictionary<Point, Tile>(); //initialize

            RedrawBoard();  //calling this again to show tiles.
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
            //redraw everything?
            RedrawBoard();
        }

        /// <summary>
        /// Some other player, p, has dumped some of his or her letters. Not much to do here, 
        /// maybe just log it to a text-status output window thingy.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        private void DrawOpponentTurn(DumpLetters t, Player p)
        {
            StatusBar.Text = string.Format("Player {0} dumped some letters...", p.Name);
        }

        /// <summary>
        /// Opponent p has passed. Not much to do here, just log it to some kind of text status window/control.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        private void DrawOpponentTurn(Pass t, Player p)
        {
            StatusBar.Text = string.Format("Player {0} has passed...", p.Name);
        }

        private void Done_Click(object sender, RoutedEventArgs e)
        {
            if (WordInPlay.Count > 0)
            {
                //Can we expose constr argument as a different type?
                //PlaceMove pm = new PlaceMove()
            }
            else
            {
                MessageBox.Show("You should probably try placing some letters first...");
            }
            NotifyTurn();
        }
        private void Dump_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerTiles.PlayerTiles.Count > 0)
            {
                Scrabble.Core.Types.DumpLetters d = new DumpLetters(
                    PlayerTiles.PlayerTiles.ConvertAll<Scrabble.Core.Types.Tile>
                        (t => { return new Scrabble.Core.Types.Tile(t.Letter[0]); })
                    );

                NotifyTurn();
            }
            else
            {
                MessageBox.Show("You have no letters in your tray.");
            }
        }
        private void Pass_Click(object sender, RoutedEventArgs e)
        {
            Scrabble.Core.Types.Pass p = new Core.Types.Pass();
            Player.TakeTurn(p);
            NotifyTurn();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            foreach (KeyValuePair<Point, Tile> playedTile in WordInPlay)
            {
                //remove from display board
                GameBoard.SquareAt((int)playedTile.Key.X, (int)playedTile.Key.Y).ClearSquare();

                //put back in tile rack
                PlayerTiles.PlayerTiles.Add(playedTile.Value);
            }

            //update UI
            RedrawBoard();
        }

        private void RedrawBoard()
        {
            //redraw whole board based on current state
            GameBoard.UpdateSquares(Game.Instance.PlayingBoard);
            
            //i hope you have tiles... cuz the UI is getting rebuilt           
            //todo: maybe rename this property, it's confusing
            PlayerTiles.PlayerTiles.Clear();
            foreach (Scrabble.Core.Types.Tile t in this.Player.Tiles)
            {
                PlayerTiles.PlayerTiles.Add(new Tile(t.Letter.ToString(), t.Score));
            }
            PlayerTiles.Redraw();
        }
        
        private void ButtonsOn(bool on)
        {
            Done.IsEnabled = Dump.IsEnabled = Pass.IsEnabled = Cancel.IsEnabled = on;

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
            ButtonsOn(true);
        }

        /// <summary>
        /// It's your turn to act, create a new Turn object and call this.Player.TakeTurn(). Also, you should
        /// update the GUI to reflect this player's actions before calling this.Player.TakeTurn().
        /// </summary>
        public void NotifyTurn()
        {
            //turn should be taken in the Done, Dump, or Pass handlers

            //Redraw entire board
            RedrawBoard();
            
            //you're done, enable buttons again
            ButtonsOn(false);
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
            if (o.Winners.Contains(Player))
            {
                StatusBar.Text = "You won.  Congratulations.  Banana Stickers and Beer Tickets for everyone!";
            }
            else
            {
                //aggregate may be incorrect...  can't test right now
                StatusBar.Text = string.Format("{0} won.  Better luck next time.", o.Winners.Aggregate("", (x,y) => { return x + " & " + y.Name;}));
            }
        }

        #endregion

        #region Turn State Properties

        public Dictionary<Point, Tile> WordInPlay { get; set; }

        #endregion

        
    }
}
