namespace SpecCoreLib.Demo.SpaceRaidersGame.Entities
{
    using System.Windows.Media;
    using SpecCoreLib;

    /// <summary>
    /// Represents an enemy bomb
    /// </summary>
    public class EnemyBomb
    {
        private readonly SpeccyEngine _speccy;

        public int Col { get; }

        public int Row { get; private set; }

        public EnemyBomb(SpeccyEngine speccy, int row, int col)
        {
            _speccy = speccy;
            Col = col;
            Row = row;
        }

        public void Draw()
        {
            if (Row >= 39)
            {
                return;
            }

            _speccy.Print(Row, Col, " ");

            Row++;
            _speccy.Pen = Colors.Red;
            _speccy.Print(Row, Col, "o");
        }
    }
}
