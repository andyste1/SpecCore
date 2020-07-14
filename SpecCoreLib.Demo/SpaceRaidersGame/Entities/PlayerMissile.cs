namespace SpecCoreLib.Demo.SpaceRaidersGame.Entities
{
    using System.Windows.Media;
    using SpecCoreLib;

    /// <summary>
    /// Responsible for rendering and hit testing a player's missile
    /// </summary>
    public class PlayerMissile
    {
        private readonly SpeccyEngine _speccy;
        
        public int Col { get; }

        public int Row { get; private set; }

        public PlayerMissile(SpeccyEngine speccy, int row, int col)
        {
            _speccy = speccy;
            Col = col;
            Row = row;
        }

        public void Draw()
        {
            if (Row < 0)
            {
                return;
            }

            _speccy.Print(Row, Col, " ");

            Row--;
            _speccy.Pen = Colors.Orange;
            _speccy.Print(Row, Col, "*");
        }
    }
}
