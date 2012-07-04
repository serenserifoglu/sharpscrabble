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

        }

        public Tile(string letter, int score) : this()
        {
            Letter = letter;
            Score = score;
            Redraw();
        }

        public void Redraw()
        {
            DisplayLetter.Text = Letter;
            DisplayScore.Text = Score.ToString();
        }

        //drag/drop
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            Tiles t = UtilityFunctions.GetAncestorOfType<Tiles>(this);
            GameWindow w = UtilityFunctions.GetAncestorOfType<GameWindow>(this);
            if (t != null || ((w != null) && w.WordInPlay.ContainsValue(this)))
            {
                DataObject thisTileData = new DataObject("scTile", this);
                DragDrop.DoDragDrop(this, thisTileData, DragDropEffects.Move);
            }
        }


        public string Letter { get; set; }
        public int Score { get; set; }
    }
}
