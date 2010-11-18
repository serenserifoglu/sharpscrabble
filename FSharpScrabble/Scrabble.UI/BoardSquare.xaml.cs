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

namespace Scrabble.UI
{
    /// <summary>
    /// Interaction logic for Square.xaml
    /// </summary>
    public partial class BoardSquare : UserControl
    {
        public BoardSquare()
        {
            InitializeComponent();

            this.AllowDrop = true;
            this.Drop += new DragEventHandler(BoardSquare_Drop);
        }


        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            SquareContainer.Background = Resources["HoverSquare"] as Brush;
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);
            SquareContainer.Background = Resources["InterestingSquare"] as Brush;
        }

        //drag/drop code
        void BoardSquare_Drop(object sender, DragEventArgs e)
        {
            Tile t = (Tile)e.Data.GetData("scTile");
            StackPanel letterTray = (StackPanel)t.Parent;
            Grid layout = (Grid)letterTray.Parent;
            Tiles these = (Tiles)layout.Parent;
            these.PlayerTiles.Remove(t);
            letterTray.Children.Remove(t);
            if (!Put(t))
            {
                //put stuff back noob
                these.PlayerTiles.Add(t);
                letterTray.Children.Add(t);
            }
            //change bg back
            SquareContainer.Background = Resources["InterestingSquare"] as Brush;

            Redraw();
        }

        public bool Put(Tile sqToPlace)
        {
            if (PlacedTile == null)
            {
                PlacedTile = sqToPlace;
                UtilityFunctions.GetAncestorOfType<GameWindow>(this).WordInPlay.Add(
                    this.MyCoords,
                    sqToPlace);
                return true;
            }
            else
            {
                MessageBox.Show("There's already a tile there.  Wrong game, dude.");
                return false;
            }
        }

        public void ClearSquare()
        {
            this.PlacedTile = null;
            SquareContainer.Children.Clear();
        }

        public void Redraw()
        {
            if (PlacedTile != null)
            {
                SquareContainer.Children.Clear();
                SquareContainer.Children.Add(PlacedTile);
            }
        }

        public Point MyCoords { get; set; }
        public Tile PlacedTile { get; set; }
    }
}
