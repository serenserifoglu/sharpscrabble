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
using Scrabble.Core;
using Scrabble.Core.Types;
using Scrabble.Core.Squares;

namespace Scrabble.UI
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class DisplayBoard : UserControl
    {
        public DisplayBoard()
        {
            InitializeComponent();
            
            //create list o' squares
            _allSquares = new BoardSquare[15, 15];
            //initialize them to blank/not null
            for (int i = 0; i < _allSquares.Length; i++) _allSquares[i/15,i%15] = new BoardSquare();
            
            Redraw();
        }

        public void Redraw()
        {
            BoardGrid.Children.Clear();
            System.Windows.GridLength g = new GridLength(40);
            
            for (int v = 0; v < 15; v++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition() { Height = g });
                for (int h = 0; h < 15; h++)
                {
                    BoardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = g });
                    BoardSquare square = _allSquares[h, v];
                    if (v == 7 && h == 7)
                    {
                        square.Background = this.Resources["CenterSquare"] as Brush;
                    }
                    if (square.PlacedTile != null)
                    {
                        square.Redraw();
                    }

                    square.MyCoords = new Point(h, v);
                    
                    BoardGrid.Children.Add(square);

                    //horiz(x), vert(y)
                    Grid.SetColumn(square, h);
                    Grid.SetRow(square, v);
                }
            }

        }

        public void UpdateSquares(Board instanceBoard)
        {
            for (int i = 0; i < _allSquares.LongLength; i++)
            {
                int y = i%15; //rows
                int x = i/15; //columns
                Square s = instanceBoard.Get(x,y);
                //remove tile
                _allSquares[x,y].PlacedTile = null;
                //add tile if exists
                if (!s.IsEmpty)
                {
                    Scrabble.Core.Types.Tile t = (Scrabble.Core.Types.Tile)s.Tile;
                    _allSquares[x, y].PlacedTile = new Tile(t.Letter.ToString(), t.Score);
                }
                else if (s.WordMultiplier > 0)
                {
                    //special square, need to display accordingly
                }
                else if (s.LetterMultiplier > 0)
                {
                    //same deal, btw i hate else-if statements
                }
                    

            }
            Redraw();
                
        }

        public BoardSquare SquareAt(int column, int row)
        {
            //x-col, y-row
            return _allSquares[column, row];    
        }

        private BoardSquare[,] _allSquares;
    }
}
