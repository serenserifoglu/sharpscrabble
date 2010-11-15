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
    public partial class DisplayBoard : UserControl
    {
        public DisplayBoard()
        {
            InitializeComponent();
            
            //create list o' squares
            _allSquares = new BoardSquare[15, 15];
            //initialize them to blank
            for (int i = 0; i < _allSquares.Length; i++) _allSquares[i/15,i%15] = new BoardSquare();
            
            Redraw();
        }

        public void Redraw()
        {
            BoardGrid.Children.Clear();
            System.Windows.GridLength g = new GridLength(40);
            
            for (int h = 0; h < 15; h++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition() { Height = g });
                for (int v = 0; v < 15; v++)
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
