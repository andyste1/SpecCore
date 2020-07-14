namespace SpecCoreLib.Demo.SabotageGame
{
    using System;
    using System.Threading;
    using System.Windows.Media;
    using SpecCoreLib.Demo.Utils;

    public class SecurityGuard
    {
        private readonly SpeccyEngine _speccyEngine;
        private readonly AStarMazeSolver _mazeSolver;
        private readonly bool[,] _map;

        public int Row { get; private set; }
        public int Col { get; private set; }
        public bool KilledInExplosion { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGuard" /> class.
        /// </summary>
        /// <param name="speccyEngine">The speccy engine.</param>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        /// <param name="map">The map.</param>
        public SecurityGuard(
            SpeccyEngine speccyEngine, 
            int row,
            int col,
            bool[,] map)
        {
            _speccyEngine = speccyEngine;
            _map = map;
            Row = row;
            Col = col;

            _mazeSolver = new AStarMazeSolver(map, timeoutMs: 10);
        }

        /// <summary>
        /// Draws the character, moving it towards the player.
        /// </summary>
        /// <param name="playerRow">The player row.</param>
        /// <param name="playerCol">The player col.</param>
        public void Draw(int playerRow, int playerCol)
        {
            var path = _mazeSolver.Solve(
                Col,
                Row,
                playerCol,
                playerRow);
            var nextMove = FindParent(path);

            _speccyEngine.Pen = Colors.DodgerBlue;
            _speccyEngine.Print(Row, Col, " ");

            Row = nextMove.Y;
            Col = nextMove.X;
            _speccyEngine.Print(Row, Col, "¬a");

            //_speccyEngine.Paper = Colors.Transparent;
            //var start = true;
            //while (path != null)
            //{
            //    _speccyEngine.Pen = start ? Colors.Yellow : Colors.Red;
            //    _speccyEngine.Print(path.Y, path.X, "+");
            //    path = path.Parent;
            //    start = false;
            //}
        }

        /// <summary>
        /// The node returned by the maze solver is the one nearest the player, so we must traverse the graph
        /// in reverse until we find the *second to top*, which will be where the guard needs to move to
        /// (the very first node is the guard's current position).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private Location FindParent(Location path)
        {
            if (path.Parent == null || (path.Parent.X == Col && path.Parent.Y == Row))
            {
                return path;
            }

            return FindParent(path.Parent);
        }

        /// <summary>
        /// Checks if the guard and player are on the same horizontal or vertical, and shoots if there is a clear
        /// line of sight.
        /// </summary>
        /// <param name="playerRow">The player row.</param>
        /// <param name="playerCol">The player col.</param>
        /// <returns>True if the player was shot</returns>
        public bool ShootIfClear(in int playerRow, in int playerCol)
        {
            if (Row == playerRow && Col == playerCol)
            {
                return true;
            }

            if (Row == playerRow)
            {
                var start = Math.Min(Col, playerCol);
                var end = Math.Max(Col, playerCol);
                for (var i = start + 1; i < end; i++)
                {
                    if (_map[Row, i])
                    {
                        return false;
                    }
                }
            }
            else if (Col == playerCol)
            {
                var start = Math.Min(Row, playerRow);
                var end = Math.Max(Row, playerRow);
                for (var i = start + 1; i < end; i++)
                {
                    if (_map[i, Col])
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }

            // If we've reached here then there must be a clear line of sight, so take the shot!
            var pr = playerRow;
            var pc = playerCol;
            _speccyEngine.RunOutOfFrameAnimationAsync(() =>
            {
                var x1 = Col * 8 + 4;
                var y1 = Row * 8 + 4;
                var x2 = pc * 8 + 4;
                var y2 = pr * 8 + 4;

                for (var i = 0; i < 20; i++)
                {
                    _speccyEngine.Pen = _speccyEngine.GetRandomColour(includeBlack: false);
                    _speccyEngine.Line(x1, y1, x2, y2);
                    Thread.Sleep(10);
                    _speccyEngine.ForceFrame();
                }
            });

            return true;
        }
    }
}
