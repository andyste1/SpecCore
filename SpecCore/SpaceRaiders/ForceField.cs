namespace SpecCore.SpaceRaiders
{
    using System.Windows.Media;
    using SpecCoreLib;

    /// <summary>
    /// Responsible for drawing and animating the forcefield, and managing "hits" to it.
    /// </summary>
    public class ForceField
    {
        private const string UnbreakableBlockGlyph = "¬K";
        private const string NormalBlockGlyph = "¬V";
        private const string MissingBlockGlyph = " ";

        private readonly SpeccyEngine _speccy;
        private readonly BlockStrength[] _forceField = new BlockStrength[SpeccyEngine.ScreenCols];
        private readonly int _row;

        private int _rotateStart;

        private enum BlockStrength
        {
            Missing,
            Normal,
            Unbreakable
        }

        public ForceField(SpeccyEngine speccy, int row)
        {
            _speccy = speccy;
            _row = row;
            Initialise();
        }

        public void Draw()
        {
            _speccy.Paper = Colors.Black;
            _speccy.Pen = Colors.Red;

            _rotateStart++;
            if (_rotateStart == SpeccyEngine.ScreenCols)
            {
                _rotateStart = 0;
            }

            var i = _rotateStart;
            for (var col = 0; col < SpeccyEngine.ScreenCols; col++)
            {
                switch (_forceField[i])
                {
                    case BlockStrength.Normal:
                        _speccy.Print(_row, col, NormalBlockGlyph);
                        break;
                    case BlockStrength.Missing:
                        _speccy.Print(_row, col, MissingBlockGlyph);
                        break;
                    case BlockStrength.Unbreakable:
                        _speccy.Print(_row, col, UnbreakableBlockGlyph);
                        break;
                }

                i++;
                if (i == _forceField.Length)
                {
                    i = 0;
                }
            }
        }

        public ProjectileHitTestResult HitTest(PlayerMissile playerMissile)
        {
            if (playerMissile.Row != _row)
            {
                return new ProjectileHitTestResult();
            }

            // Adjust the missile's column to take into account the rotation of the force field.
            var adjCol = playerMissile.Col + _rotateStart;
            if (adjCol >= SpeccyEngine.ScreenCols)
            {
                adjCol = adjCol - SpeccyEngine.ScreenCols;
            }

            switch (_forceField[adjCol])
            {
                case BlockStrength.Unbreakable:
                    return new ProjectileHitTestResult { ProjectileStopped = true };

                case BlockStrength.Normal:
                    _forceField[adjCol] = BlockStrength.Missing;
                    return new ProjectileHitTestResult { ProjectileStopped = true, Score = 10 };

                default:
                    return new ProjectileHitTestResult();
            }
        }

        private void Initialise()
        {
            const int UnbreakableInterval = 5;

            for (var i = 0; i < SpeccyEngine.ScreenCols; i++)
            {
                _forceField[i] = i % UnbreakableInterval == 0 
                    ? BlockStrength.Unbreakable 
                    : BlockStrength.Normal;
            }

            _rotateStart = 0;
        }
    }
}
