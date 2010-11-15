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
            //initialize blank
            PlayerTiles = new List<Tile>();
            
            Redraw();
        }

        public void Redraw()
        {
            if(TileRack.Children.Count > 0) TileRack.Children.Clear();
            
            foreach (Tile tile in PlayerTiles)
            {
                TileRack.Children.Add(tile);
            }
            
        }

        public List<Tile> PlayerTiles { get; set; }


        public string PlayerName { 
            get { return (string)GetValue(_playerName); }
            set { SetValue(_playerName, value); }
        }

        public static readonly DependencyProperty _playerName =
            DependencyProperty.Register("PlayerName", typeof(string), typeof(Tiles));
    }
}
