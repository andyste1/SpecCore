namespace SpecCoreLib.Demo
{
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            //new TestCard.TestCard(this);
            //new MazeGame.MazeGame(this);
            //new SpaceRaidersGame.SpaceRaidersGame(this);
            new SabotageGame.SabotageGame(this);

            InitializeComponent();
        }
    }
}
