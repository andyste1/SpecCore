using System;
using System.Collections.Generic;

namespace SpecCoreLib.Demo.SabotageGame
{
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Maze solver using the A* algorithm.
    /// </summary>
    public class AStarMazeSolver
    {
        private readonly bool[,] _map;
        private readonly int _timeoutMs;

        /// <summary>
        /// Initializes a new instance of the <see cref="AStarMazeSolver" /> class.
        /// </summary>
        /// <param name="map">The map, as a 2D bool array. The first dimension are the rows (Y) and
        /// second are the columns (X). False indicates a "space" while true is a "wall".</param>
        /// <param name="timeoutMs">The timeout ms. If the player is far away or inaccessible, the algorithm
        /// can take time to run. The timeout ensures it doesn't take too long, but this can result in an
        /// incomplete path being created, which most of the time isn't an issue in "chase" games.
        /// Pass a value of 0 or less to have no timeout.</param>
        /// <returns>The path hierarchy</returns>
        public AStarMazeSolver(bool[,] map, int timeoutMs)
        {
            _map = map;
            _timeoutMs = timeoutMs;
        }

        /// <summary>
        /// Solves the maze.
        /// </summary>
        /// <param name="startX">The start x.</param>
        /// <param name="startY">The start y.</param>
        /// <param name="targetX">The target x.</param>
        /// <param name="targetY">The target y.</param>
        public Location Solve(
            int startX, 
            int startY, 
            int targetX, 
            int targetY)
        {
            Location current = null;
            var openList = new List<Location>();
            var closedList = new List<Location>();
            var g = 0;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var start = new Location { X = startX, Y = startY };
            var target = new Location { X = targetX, Y = targetY };

            // start by adding the original position to the open list  
            openList.Add(start);

            while (openList.Count > 0)
            {
                // get the square with the lowest F score  
                var lowest = openList.Min(l => l.F);
                current = openList.First(l => l.F == lowest);

                // add the current square to the closed list  
                closedList.Add(current);

                // remove it from the open list  
                openList.Remove(current);

                // if we added the destination to the closed list, we've found a path  
                if (closedList.FirstOrDefault(l => l.X == target.X && l.Y == target.Y) != null)
                {
                    break;
                }

                var adjacentSquares = GetWalkableAdjacentSquares(
                    current, 
                    target,
                    openList);
                g = current.G + 1;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it  
                    if (closedList.FirstOrDefault(l => l.X == adjacentSquare.X && l.Y == adjacentSquare.Y) != null)
                    {
                        continue;
                    }

                    // if it's not in the open list...  
                    if (openList.FirstOrDefault(l => l.X == adjacentSquare.X && l.Y == adjacentSquare.Y) == null)
                    {
                        // compute its score, set the parent  
                        adjacentSquare.G = g;
                        adjacentSquare.H = ComputeHScore(adjacentSquare.X, adjacentSquare.Y, target.X, target.Y);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;

                        // and add it to the open list  
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // test if using the current G score makes the adjacent square's F score  
                        // lower, if yes update the parent because it means it's a better path  
                        if (g + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g;
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = current;
                        }
                    }
                }

                if (_timeoutMs > 0 && stopwatch.ElapsedMilliseconds > _timeoutMs)
                {
                    break;
                }
                //if (CountPathLength(current) > 10)
                //{
                //    // Rather than calculate the path all the way to the player, stop after finding N moves.
                //    break;
                //}
            }

            return current;
        }

        private int CountPathLength(Location current)
        {
            var count = 0;
            var temp = current;

            do
            {
                count++;
                temp = temp.Parent;
            } while (temp != null);

            return count;
        }

        /// <summary>
        /// Gets the walkable adjacent squares.
        /// </summary>
        /// <param name="current">The current.</param>
        /// <param name="target">The target.</param>
        /// <param name="openList">The open list.</param>
        /// <returns></returns>
        private List<Location> GetWalkableAdjacentSquares(
            Location current,
            Location target,
            List<Location> openList)
        {
            var x = current.X;
            var y = current.Y;

            if (x < 0 || y < 0 || x >= SpeccyEngine.ScreenCols && y >= SpeccyEngine.ScreenRows)
            {
                return new List<Location>();
            }

            var list = new List<Location>();

            if (y > 0 && (_map[y - 1, x] == false || (y - 1 == target.Y && x == target.X)))
            {
                var node = openList.Find(l => l.X == x && l.Y == y - 1);
                list.Add(node ?? new Location() { X = x, Y = y - 1 });
            }

            if (y < SpeccyEngine.ScreenRows - 1 && (_map[y + 1, x] == false || (y + 1 == target.Y && x == target.X)))
            {
                var node = openList.Find(l => l.X == x && l.Y == y + 1);
                list.Add(node ?? new Location() { X = x, Y = y + 1 });
            }

            if (x > 0 && (_map[y, x - 1] == false || (y  == target.Y && x - 1 == target.X)))
            {
                var node = openList.Find(l => l.X == x - 1 && l.Y == y);
                list.Add(node ?? new Location() { X = x - 1, Y = y });
            }

            if (x < SpeccyEngine.ScreenCols - 1 && (_map[y, x + 1] == false || (y == target.Y && x + 1 == target.X)))
            {
                var node = openList.Find(l => l.X == x + 1 && l.Y == y);
                list.Add(node ?? new Location() { X = x + 1, Y = y });
            }

            return list;
        }

        private static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y);
        }
    }

    public class Location
    {
        public int X;
        public int Y;
        public int F;
        public int G;
        public int H;
        public Location Parent;
    }
}
