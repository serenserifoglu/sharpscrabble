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
            Redraw();
        }

        public void Redraw()
        {
            System.Windows.GridLength g = new GridLength(40);
            //silly test loop
            for (int h = 0; h < 15; h++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition() { Height = g });
                for (int v = 0; v < 15; v++)
                {
                    BoardGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = g });
                    BoardSquare square = new BoardSquare();
                    BoardGrid.Children.Add(square);
                    Grid.SetColumn(square, v);
                    Grid.SetRow(square, h);
                }
            }

        }


    }
}
