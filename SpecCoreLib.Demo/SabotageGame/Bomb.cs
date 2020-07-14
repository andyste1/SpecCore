namespace SpecCoreLib.Demo.SabotageGame
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;

    // Used to draw the explosion and total up resulting scores.
    public class Bomb
    {
        private const int ExplosionRadius = 2;

        private readonly SpeccyEngine _speccyEngine;
        private readonly bool[,] _crateMap;
        private readonly DateTime _detonationTime;

        private int _row;
        private int _col;
        private int _blastCount;
        private int _burnoutThresholdCount;

        public bool Detonated { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the player was killed in the explosion.
        /// </summary>
        public bool PlayerKilled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the explosion has finished.
        /// </summary>
        public bool ExplosionFinished { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bomb" /> class.
        /// </summary>
        /// <param name="speccyEngine">The speccy engine.</param>
        /// <param name="crateMap">The crate map.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        public Bomb(
            SpeccyEngine speccyEngine, 
            bool[,] crateMap,
            int row, 
            int col)
        {
            _speccyEngine = speccyEngine;
            _crateMap = crateMap;
            _row = row;
            _col = col;
            _detonationTime = DateTime.Now.AddSeconds(6);
        }

        /// <summary>
        /// Draws the bomb/explosion.
        /// </summary>
        /// <param name="playerRow">The player row.</param>
        /// <param name="playerCol">The player col.</param>
        /// <param name="guards">The guards.</param>
        /// <param name="score">The score.</param>
        public void Draw(
            int playerRow, 
            int playerCol, 
            List<SecurityGuard> guards,
            ref int score)
        {
            if (ExplosionFinished)
            {
                return;
            }

            if (Detonated)
            {
                DrawExplosion(
                    playerRow, 
                    playerCol, 
                    guards,
                    ref score);
            }
            else
            {
                var countdown = _detonationTime.Subtract(DateTime.Now).TotalSeconds;

                _speccyEngine.Paper = Colors.Blue;
                _speccyEngine.Pen = Colors.OrangeRed;
                _speccyEngine.Flash = true;
                _speccyEngine.Print(0, 2, $"Countdown: {countdown:F0}");

                _speccyEngine.Paper = Colors.Yellow;
                _speccyEngine.Print(_row, _col, "*");
                _speccyEngine.Flash = false;

                if (DateTime.Now >= _detonationTime)
                {
                    _speccyEngine.Paper = Colors.Blue;
                    _speccyEngine.Print(0, 2, "             ");
                    Detonated = true;
                }
            }
        }

        /// <summary>
        /// Draws the explosion.
        /// </summary>
        /// <param name="playerRow">The player row.</param>
        /// <param name="playerCol">The player col.</param>
        /// <param name="guards">The guards.</param>
        /// <param name="score">The score.</param>
        private void DrawExplosion(
            int playerRow,
            int playerCol,
            List<SecurityGuard> guards,
            ref int score)
        {
            const int MinBlasts = 50;

            if (_blastCount > 0)
            {
                ClearBlastArea();

                var position = CalculateNextBlastPosition(7);
                _row = position.Item1;
                _col = position.Item2;
            }

            // Draw the explosion, checking for crates, guards and player.
            _speccyEngine.Paper = Colors.Yellow;
            _speccyEngine.Pen = Colors.Red;
            var numCratesDestroyed = 0;
            for (var r = _row - ExplosionRadius; r <= _row + ExplosionRadius; r++)
            {
                for (var c = _col - ExplosionRadius; c <= _col + ExplosionRadius; c++)
                {
                    if (r < 1 || r > SpeccyEngine.ScreenRows - 1 || c < 0 || c > SpeccyEngine.ScreenCols - 1)
                    {
                        continue;
                    }

                    if (_crateMap[r, c])
                    {
                        score += 10;
                        _crateMap[r, c] = false;
                        numCratesDestroyed++;
                    }
                    if (r == playerRow && c == playerCol)
                    {
                        PlayerKilled = true;
                    }

                    foreach (var guard in guards)
                    {
                        if (!guard.KilledInExplosion && r == guard.Row && c == guard.Col)
                        {
                            score += 200 * guards.Count;
                            guard.KilledInExplosion = true;
                        }
                    }

                    _speccyEngine.Print(r, c, "¬V");
                }
            }

            _blastCount++;

            // There will always be a minimum number of "blasts"; after this, we'll then start counting
            // how many blasts do <=5 crates damage each (20% of the blast area).
            // The explosion will stop once there have been 10 such "low damage" blasts.
            if (_blastCount > MinBlasts)
            {
                if (numCratesDestroyed < 5)
                {
                    _burnoutThresholdCount++;
                }

                if (_burnoutThresholdCount == 10)
                {
                    ExplosionFinished = true;
                    ClearBlastArea();
                }
            }
        }

        /// <summary>
        /// Calculates the next blast position by looking around the perimeter of the current position
        /// to see where the most crates are.
        /// </summary>
        private Tuple<int, int> CalculateNextBlastPosition(int distance)
        {
            const int SearchCount = 10;

            if (distance / 2 == distance / 2.0)
            {
                throw new ArgumentException("distance must be an odd number");
            }

            var negDistOffset = (distance - 1) / 2;

            var rnd = new Random();

            var maxCount = -1;
            var numEmptyBlasts = 0;
            Tuple<int, int> maxPP = null;
            for (var i = 0; i < SearchCount; i++)
            {
                var pp = new Tuple<int, int>(_row + rnd.Next(distance) - negDistOffset, _col + rnd.Next(distance) - negDistOffset);
                if (pp.Item1 < 1 || pp.Item1 > SpeccyEngine.ScreenRows - 1 || pp.Item2 < 0 || pp.Item2 > SpeccyEngine.ScreenCols - 1)
                {
                    continue;
                }

                var count = CountCrates(pp.Item1, pp.Item2);
                if (count > maxCount)
                {
                    maxCount = count;
                    maxPP = pp;
                }

                if (count == 0)
                {
                    numEmptyBlasts++;
                }
            }

            // If we've been moving around a particularly sparse area then widen the search
            if (numEmptyBlasts > SearchCount / 2)
            {
                return CalculateNextBlastPosition(11);
            }

            return maxPP ?? new Tuple<int, int>(_row, _col);
        }

        /// <summary>
        /// Counts the crates around the given point.
        /// </summary>
        /// <param name="centreRow">The centre row.</param>
        /// <param name="centreCol">The centre col.</param>
        /// <returns></returns>
        private int CountCrates(int centreRow, int centreCol)
        {
            var count = 0;

            for (var r = centreRow - ExplosionRadius; r <= centreRow + ExplosionRadius; r++)
            {
                for (var c = centreCol - ExplosionRadius; c <= centreCol + ExplosionRadius; c++)
                {
                    if (r >= 1 && r < SpeccyEngine.ScreenRows && c >= 0 && c < SpeccyEngine.ScreenCols)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private void ClearBlastArea()
        {
            _speccyEngine.Paper = Colors.Black;
            for (var r = _row - ExplosionRadius; r <= _row + ExplosionRadius; r++)
            {
                for (var c = _col - ExplosionRadius; c <= _col + ExplosionRadius; c++)
                {
                    _speccyEngine.Print(r, c, " ");
                }
            }
        }
    }
}
