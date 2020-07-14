namespace SpecCoreLib.Demo.MazeGame
{
    using System;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    public class MazeGame : SpeccyEngine
    {
        private const int PrizeRow = 31;
        private const int PrizeCol = 51;

        private GameState _gameState = GameState.WaitingToStart;
        private int _playerCol;
        private int _playerRow;
        private bool[,] _maze;
        private DateTime _gameStartTime;
        private TimeSpan _timeTakenToComplete;

        private enum GameState
        {
            WaitingToStart,
            Running,
            Ended
        }

        public MazeGame(Window window)
            : base(window)
        {
        }

        protected override void Init()
        {
            // We want the player to move around a little more quickly than the default 4fps.
            SetFps(10);

            var player = new[]
                {
                    "00011000",
                    "00011000",
                    "00111100",
                    "01011010",
                    "10011001",
                    "00100100",
                    "00100100",
                    "01000010",
                };
            SetGraphicGlyph('a', player);

            var jewel = new[]
                {
                    "00000000",
                    "00000000",
                    "01111110",
                    "11111111",
                    "01111110",
                    "00111100",
                    "00011000",
                    "00000000",
                };
            SetGraphicGlyph('b', jewel);
        }

        protected override void DoFrame()
        {
            switch (_gameState)
            {
                case GameState.WaitingToStart:
                    WaitForStart();
                    break;
                case GameState.Running:
                    GameLoop();
                    break;
                case GameState.Ended:
                    EndOfGame();
                    break;
            }
        }

        /// <summary>
        /// The game loop, basically handling player movement.
        /// </summary>
        private void GameLoop()
        {
            Print(_playerRow, _playerCol, " ");

            if (LastKeyPress == Key.Up && !_maze[_playerRow - 1, _playerCol])
            {
                _playerRow--;
            }
            else if (LastKeyPress == Key.Down && !_maze[_playerRow + 1, _playerCol])
            {
                _playerRow++;
            }
            else if (LastKeyPress == Key.Left && !_maze[_playerRow, _playerCol - 1])
            {
                _playerCol--;
            }
            else if (LastKeyPress == Key.Right && !_maze[_playerRow, _playerCol + 1])
            {
                _playerCol++;
            }

            Pen = Colors.Cyan;
            Print(_playerRow, _playerCol, "¬a");

            if (_playerRow == PrizeRow && _playerCol == PrizeCol)
            {
                _gameState = GameState.Ended;
                _timeTakenToComplete = DateTime.Now.Subtract(_gameStartTime);
            }
        }

        /// <summary>
        /// Runs when the game is waiting for the user to press a key to start it.
        /// </summary>
        private void WaitForStart()
        {
            Clear(Colors.Black);
            Paper = Colors.Black;
            Pen = Colors.Cyan;
            Print(5, 10, "Find your way to the centre of the maze");
            Print(6, 16, "Use the cursor keys to move");

            PrepareForStart();
        }

        /// <summary>
        /// Handles the end of the game.
        /// </summary>
        private void EndOfGame()
        {
            Clear(Colors.Black);

            Paper = SpectrumColours[new Random().Next(SpectrumColours.Length)];
            Pen = Colors.Black;
            Print(5, 18, "   G A M E   O V E R   ");

            Paper = Colors.Black;
            Pen = Colors.White;
            Print(7, 17, "You completed the game in:");
            Print(8, 26, _timeTakenToComplete.ToString(@"hh\:mm\:ss"));

            PrepareForStart();
        }

        /// <summary>
        /// Displays the "press any key..." message and waits for a keypress. Used during startup and at the
        /// end of the game.
        /// </summary>
        private void PrepareForStart()
        {
            Pen = Colors.Black;
            Paper = Colors.Yellow;
            Print(10, 17, "Press any key to start...");

            if (LastKeyPress != Key.None)
            {
                GenerateMaze();
                DrawMaze();
                DrawPrize();
                DropPlayer();
                _gameState = GameState.Running;
                _gameStartTime = DateTime.Now;
            }
        }

        private void DropPlayer()
        {
            // Put the player in an empty block close to the top-left corner.
            for (var r = 0; r < 5; r++)
            {
                for (var c = 0; c < 5; c++)
                {
                    if (!_maze[r, c])
                    {
                        _playerRow = r;
                        _playerCol = c;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Draws the maze.
        /// </summary>
        private void DrawMaze()
        {
            Clear(Colors.Black);
            Paper = Colors.Black;
            Pen = Colors.Red;
            for (var r = 0; r < 40; r++)
            {
                for (var c = 0; c < 60; c++)
                {
                    Print(r, c, _maze[r, c] ? "¬V" : " ");
                }
            }
        }

        /// <summary>
        /// Draws the "prize" somewhere near the lower-right corner of the maze.
        /// </summary>
        private void DrawPrize()
        {
            // Clear a space
            for (var r = 30; r <= 32; r++)
            {
                for (var c = 50; c <= 52; c++)
                {
                    _maze[r, c] = false;
                    Print(r, c, " ");
                }
            }

            Pen = Colors.Gold;
            Print(PrizeRow, PrizeCol, "¬b");
        }

        /// <summary>
        /// Generates the maze.
        /// </summary>
        private void GenerateMaze()
        {
            Clear(Colors.Black);

            _maze = new bool[40, 60];
            for (var row = 0; row < 39; row++)
            {
                for (var col = 0; col < 59; col++)
                {
                    _maze[row, col] = true; // true=wall
                }
            }

            var rnd = new Random();
            var r = rnd.Next(40);
            var c = rnd.Next(60);

            // Must be odd
            if (r % 2 == 0)
            {
                r++;
            }
            if (c % 2 == 0)
            {
                c++;
            }

            _maze[r, c] = false;
            var done = false;
            while (!done)
            {
                for (var i = 0; i < 100; i++)
                {
                    var oldr = r;
                    var oldc = c;

                    // Move in a random direction
                    switch (rnd.Next(4))
                    {
                        case 0:
                            if (c + 2 < 59)
                            {
                                c += 2;
                            }
                            break;
                        case 1:
                            if (r + 2 < 39)
                            {
                                r += 2;
                            }
                            break;
                        case 2:
                            if (c - 2 > 0)
                            {
                                c -= 2;
                            }
                            break;
                        case 3:
                            if (r - 2 > 0)
                            {
                                r -= 2;
                            }
                            break;
                    }

                    if (_maze[r, c])
                    {
                        // If a cell is unvisited then connect it
                        _maze[r, c] = false;
                        _maze[(r + oldr) / 2, (c + oldc) / 2] = false;
                    }
                }

                // Check all cells have been visited
                done = true;
                for (var c1 = 1; c1 < 59; c1 += 2)
                {
                    for (var r1 = 1; r1 < 39; r1 += 2)
                    {
                        if (_maze[r1, c1])
                        {
                            done = false;
                        }
                    }
                }
            }
        }
    }
}
