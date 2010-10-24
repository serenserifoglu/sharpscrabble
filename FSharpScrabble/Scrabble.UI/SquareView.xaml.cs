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
using Scrabble.Core.Squares;

namespace Scrabble.UI
{
    /// <summary>
    /// Displays a blank square of the board.
    /// </summary>
    public partial class SquareView : UserControl
    {
        private const string TEXT_FORMAT = "{0}x";

        public Square Square { get; set; }

        public SquareView()
        {
            InitializeComponent();    
        }

        public SquareView(Square s)
            : this()
        {
            this.Square = s;
        }

        public void Redraw()
        {
            if (Square != null)
            {
                gradient.Color = Square.Gradient.ToColor();

                if (Square is StartSquare)
                {
                    multiplier.Visibility = Visibility.Hidden;
                }
                else
                {
                    int m = Square.LetterMultiplier * Square.WordMultiplier;
                    if (m > 1)
                        multiplier.Text = String.Format(TEXT_FORMAT, m);
                    else
                        multiplier.Visibility = Visibility.Hidden;
                }
            }
            else
            {
                gradient.Color = Color.FromRgb(255, 255, 255);
                multiplier.Visibility = Visibility.Hidden;
            }
        }
    }
}
