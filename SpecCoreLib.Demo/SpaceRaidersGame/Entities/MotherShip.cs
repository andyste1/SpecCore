namespace SpecCoreLib.Demo.SpaceRaidersGame.Entities
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Windows.Media;
    using SpecCoreLib;

    /// <summary>
    /// Responsible for drawing and animating the mothership, and managing "hits" to it.
    /// </summary>
    public class MotherShip
    {
        private readonly SpeccyEngine _speccy;
        private readonly BlockTypes[,] _blockData = new BlockTypes[9, 60];
        private readonly Random _rnd = new Random();

        private int _firstRow;
        private DateTime _lastBombCreation = DateTime.MinValue;
        private bool _isCrashing;

        /// <summary>
        /// Gets the bombs.
        /// </summary>
        public EnemyBomb[] Bombs { get; }

        private enum BlockTypes
        {
            Missing, // Must be first
            Solid,
            Broken,
            Reactor
        }

        public MotherShip(SpeccyEngine speccy, int firstRow)
        {
            _speccy = speccy;
            _firstRow = firstRow;

            Bombs = new EnemyBomb[10];

            Initialise();
        }

        public void Draw()
        {
            var startRow = _firstRow;

            for (var row = 0; row < _blockData.GetLength(0); row++)
            {
                for (var col = 0; col < _blockData.GetLength(1); col++)
                {
                    switch (_blockData[row, col])
                    {
                        case BlockTypes.Solid:
                            _speccy.Pen = Colors.White;
                            _speccy.Print(startRow, col, "¬K");
                            break;
                        case BlockTypes.Broken:
                            _speccy.Pen = Colors.White;
                            _speccy.Print(startRow, col, "¬V");
                            break;
                        case BlockTypes.Missing:
                            _speccy.Print(startRow, col, " ");
                            break;
                        case BlockTypes.Reactor:
                            _speccy.Pen = Colors.Yellow;
                            _speccy.Print(startRow, col, "*");
                            break;
                    }
                }

                startRow++;
            }

            ManageBombs();
        }

        public ProjectileHitTestResult HitTest(PlayerMissile playerMissile)
        {
            var rowAdj = playerMissile.Row - _firstRow;
            if (rowAdj < 0 || rowAdj >= _blockData.GetLength(0))
            {
                return new ProjectileHitTestResult();
            }

            switch (_blockData[rowAdj, playerMissile.Col])
            {
                case BlockTypes.Solid:
                    _blockData[rowAdj, playerMissile.Col] = BlockTypes.Broken;
                    return new ProjectileHitTestResult { ProjectileStopped = true, Score = 10 };

                case BlockTypes.Broken:
                    _blockData[rowAdj, playerMissile.Col] = BlockTypes.Missing;
                    return new ProjectileHitTestResult { ProjectileStopped = true, Score = 10 };

                case BlockTypes.Reactor:
                    return new ProjectileHitTestResult { ProjectileStopped = true, HitEnemyReactor = true, Score = 100 };

                default:
                    return new ProjectileHitTestResult();
            }
        }

        private void Initialise()
        {
            // The ship design. X=solid block, !=reactor core.
            const char Block = 'X';
            const char Reactor = '!';
            const string Map = @"
                            XXXX                           
                       XXXXXXXXXXXXXX
             XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
         XXXXXXXXXXXXXXXXXXX !! XXXXXXXXXXXXXXXXXXX
         XXXXXXXXXXXXXXXXXXX    XXXXXXXXXXXXXXXXXXX
             XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
         XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
           XXX  XXX  XXX   XXXXXX   XXX  XXX  XXX  
           XXX  XXX  XXX   XXXXXX   XXX  XXX  XXX  
";

            var lines = Map.Split("\r\n");
            var row = 0;
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var col = 0;
                foreach (var chr in line)
                {
                    if (chr == Block)
                    {
                        _blockData[row, col] = BlockTypes.Solid;
                    }
                    else if (chr == Reactor)
                    {
                        _blockData[row, col] = BlockTypes.Reactor;
                    }
                    else
                    {
                        _blockData[row, col] = BlockTypes.Missing;
                    }

                    col++;
                }

                row++;
            }
        }

        /// <summary>
        /// Manages the bombs.
        /// </summary>
        private void ManageBombs()
        {
            if (_isCrashing)
            {
                return;
            }

            var test = _rnd.Next(250, 3000);
            var diff = DateTime.Now.Subtract(_lastBombCreation);
            if (diff.TotalMilliseconds > test)
            {
                // Create a new bomb.
                for (var i = 0; i < Bombs.Length; i++)
                {
                    if (Bombs[i] == null)
                    {
                        CreateBomb(i);
                        break;
                    }
                }

                _lastBombCreation = DateTime.Now;
            }

            for (var i = 0; i < Bombs.Length; i++)
            {
                if (Bombs[i] == null)
                {
                    continue;
                }

                Bombs[i].Draw();
                if (Bombs[i].Row >= 39)
                {
                    Bombs[i] = null;
                }
            }
        }

        private void CreateBomb(int index)
        {
            var col = _rnd.Next(9, 51);
            Bombs[index] = new EnemyBomb(_speccy, 10, col);
        }

        public void ClearBombs()
        {
            for (var i = 0; i < Bombs.Length; i++)
            {
                Bombs[i] = null;
            }
        }

        public void Crash()
        {
            _isCrashing = true;
            _speccy.RunOutOfFrameAnimationAsync(() => CrashAnimation());
        }

        private void CrashAnimation()
        {
            var emptyLine = new string(' ', SpeccyEngine.ScreenCols);

            // Draw the mothership plumetting to the ground, flames behind it
            while (_firstRow <= 30)
            {
                Draw();

                _speccy.Print(_firstRow, 0, emptyLine);

                _speccy.Flash = true;
                for (var i = 0; i < _rnd.Next(3, 35); i++)
                {
                    var colour = _rnd.Next(2);
                    _speccy.Pen = colour == 0 ? Colors.Orange : Colors.Yellow;

                    _speccy.Print(_firstRow, _rnd.Next(9, 51), "¬W");
                }
                _speccy.Flash = false;

                _speccy.ForceFrame();
                Thread.Sleep(100);

                _firstRow++;
            }

            // Explode/fade out
            Explode(Colors.Yellow);
            Explode(Colors.Orange);
            Explode(Colors.Red);
            Explode(Colors.Black);
        }

        private void Explode(Color colour)
        {
            var emptyLine = string.Concat(Enumerable.Repeat("¬V", SpeccyEngine.ScreenCols));

            _speccy.Pen = colour;
            for (var r = 38; r >= 0; r--)
            {
                _speccy.Print(r, 0, emptyLine);
                _speccy.ForceFrame();
                Thread.Sleep(10);
            }
        }
    }
}
