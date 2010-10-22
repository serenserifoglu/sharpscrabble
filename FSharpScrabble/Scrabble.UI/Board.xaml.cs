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

namespace Scrabble.UI
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class Board : UserControl
    {
        

        public Board()
        {
            InitializeComponent();
            
            //create list o' squares
            _allSquares = new BoardSquare[15,15];

            TestStuff();

            Redraw();
        }

        private void TestStuff()
        {
            //just for testing
            for (int i = 0; i < 15; i++) for (int j = 0; j < 15; j++) _allSquares[i, j] = new BoardSquare();

            SquareAt(1, 1).Put(new Tile("C", 8));
            SquareAt(1, 2).Put(new Tile("A", 8));
            SquareAt(1, 3).Put(new Tile("N", 8));
            SquareAt(1, 4).Put(new Tile("H", 8));
            SquareAt(1, 5).Put(new Tile("A", 8));
            SquareAt(1, 6).Put(new Tile("Z", 8));
            SquareAt(1, 7).Put(new Tile("?", 8));
        }

        public void Redraw()
        {
            BoardGrid.Children.Clear();
            System.Windows.GridLength g = new GridLength(40);
            //silly test loop
            for (int h = 0; h < 15; h++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition() { Height = g });
                for (int v = 0; v < 15; v++)
                {
                    BoardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = g });
                    BoardSquare square = _allSquares[h, v];
                    if (v == 7 && h == 7)
                    {
                        square.Background = Application.Current.Resources["CenterSquare"] as Brush;
                    }
                    if (square.PlacedTile != null)
                    {
                        square.Redraw();
                    }
                        
                    BoardGrid.Children.Add(square);
                    Grid.SetColumn(square, v);
                    Grid.SetRow(square, h);
                }
            }

        }

        public BoardSquare SquareAt(int row, int column)
        {
            return _allSquares[row, column];    
        }

        private BoardSquare[,] _allSquares;
    }
}
