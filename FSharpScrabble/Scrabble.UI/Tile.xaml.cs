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
    /// Interaction logic for Tile.xaml
    /// </summary>
    public partial class Tile : UserControl
    {
        public Tile()
        {
            InitializeComponent();
            Redraw();
        }

        public Tile(string letter, int score) : this()
        {
            Letter = letter;
            Score = score;
        }

        public void Redraw()
        {
            DisplayLetter.Text = Letter;
        }
        public string Letter { get; set; }
        public int Score { get; set; }
    }
}
