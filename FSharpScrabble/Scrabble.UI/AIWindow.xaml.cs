﻿using System;
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
    /// Interaction logic for AIWindow.xaml
    /// </summary>
    public partial class AIWindow : Window, IDispWindow
    {
        //Each player gets one of these, they "own" it
        public AIWindow(ComputerPlayer p)
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
            RedrawTiles();
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
               

        private void RedrawBoard()
        {
            //redraw whole board based on current state
            GameBoard.UpdateSquares(Game.Instance.PlayingBoard);

            UpdateScores();
        }

        private void RedrawTiles()
        {
            //i hope you have tiles... cuz the UI is getting rebuilt           
            //todo: maybe rename this property, it's confusing
            PlayerTiles.PlayerTiles.Clear();
            //BUG:player.tiles is coming back with 10+ tiles in some cases
            foreach (Scrabble.Core.Types.Tile t in this.Player.Tiles)
            {
                PlayerTiles.PlayerTiles.Add(new Tile(t.Letter.ToString(), t.Score));
            }
            PlayerTiles.Redraw();

            UpdateScores();
        }

        private void UpdateScores()
        {
            /* This needs to get refactored to support > 2 players. */
            Player first = Game.Instance.Players.First();
            Player second = Game.Instance.Players.Skip(1).First();
            Player1Score.Text = string.Format("{0}: {1}", first.Name, first.Score);
            Player2Score.Text = string.Format("{0}: {1}", second.Name, second.Score);
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
            
            //System.Threading.ThreadStart st = new System.Threading.ThreadStart(
            //    delegate {
            //        this.Dispatcher.Invoke(
            //            System.Windows.Threading.DispatcherPriority.Normal,
            //            new Action(
            //                delegate {
            //                    DrawOpponentTurn(dynamicTurn, p);
            //                }));
            //    });

            //System.Threading.Thread th = new System.Threading.Thread(st);

            //th.Start();
            
            DrawOpponentTurn(dynamicTurn, p);
        }

        /// <summary>
        /// It's your turn to act, create a new Turn object and call this.Player.TakeTurn(). Also, you should
        /// update the GUI to reflect this player's actions before calling this.Player.TakeTurn().
        /// </summary>
        public void NotifyTurn()
        {
            //Redraw entire board
            RedrawBoard();

            this.Activate();
        }

         /// <summary>
        /// The player who owns this window
        /// </summary>
        public ComputerPlayer Player { get; set; }
        
        /// <summary>
        /// This gets called when the game is finished. The parameter has the winning player(s).
        /// </summary>
        /// <param name="o"></param>
        public void GameOver(GameOutcome o)
        {
            RedrawBoard();
            RedrawTiles();
            if (o.Winners.Contains(Player))
            {
                StatusBar.Text = "You won.  Congratulations.  Banana Stickers and Beer Tickets for everyone!";
            }
            else
            {
                //aggregate may be incorrect...  can't test right now
                StatusBar.Text = string.Format("{0} won.  Better luck next time.", o.Winners.Aggregate("", (x,y) => { return x + " & " + y.Name;}));
            }

            string scores = Game.Instance.Players
                .Select(p => String.Format("{0}: {1}{2}", p.Name, p.Score, Environment.NewLine))
                .Aggregate((a, b) => String.Concat(a, b));

            if (this.Player == Game.Instance.Players.Last())
                MessageBox.Show(scores, "Final Scores");
        }

        public void TilesUpdated()
        {
            RedrawTiles();
        }

        #endregion

        #region Properties

        public Dictionary<Point, Tile> WordInPlay { get; set; }

        #endregion

        
    }
}
