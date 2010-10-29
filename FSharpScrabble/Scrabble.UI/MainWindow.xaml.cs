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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WhoseTurn = 0;            
        }

        public void NextTurn()
        {
            WhoseTurn = ++WhoseTurn % NumPlayers;
        }
        
        public int NumPlayers = 2;
        public int WhoseTurn { get; set; } //player # 0 base
        public List<BoardSquare> wordInPlay { get; set; }  //what word are you trying to spell?
    }
}
