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
    /// Interaction logic for Tiles.xaml
    /// </summary>
    public partial class Tiles : UserControl
    {
        public Tiles()
        {
            InitializeComponent();
            
            //Just for testing
            PlayerTiles = new List<Tile>() { new Tile("A", 1), 
                new Tile("B", 1), new Tile("C",5 ), new Tile("D", 4), new Tile("E", 6), new Tile("F", 4), 
                new Tile("G", 5)};

            Redraw();
        }

        public void Redraw()
        {
            if(TileRack.Children.Count > 0) TileRack.Children.Clear();
            
            foreach (Tile tile in PlayerTiles)
            {
                TileRack.Children.Add(tile);
            }

            //PlayerDisplayName.Text = PlayerName;
        }

        public List<Tile> PlayerTiles { get; set; }
        public string PlayerName { get; set; }
    }
}
