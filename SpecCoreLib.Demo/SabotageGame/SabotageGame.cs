namespace SpecCoreLib.Demo.SabotageGame
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Utils;

    public class SabotageGame : SpeccyEngine
    {
        private readonly HighScoreManager _highScoreManager = new HighScoreManager(5);
        private readonly Random _rnd = new Random();

        private GameState _state;
        private int _numGuards;
        private List<SecurityGuard> _guards;
        private int _playerRow;
        private int _playerCol;
        private bool[,] _map;
        private bool _hasDrawnIntroScreen;
        private int _frameCount;
        private bool _playerKilled;
        private int _score;
        private Bomb _bomb;

        private enum GameState
        {
            IntroScreen,
            StartGame,
            InGame,
            GameOver,
        }

        public SabotageGame(Window window)
            : base(window)
        {
        }

        protected override void Init()
        {
            var figure = new[]
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
            SetGraphicGlyph('a', figure);

            var crate = new[]
                {
                    "11111111",
                    "10101010",
                    "10101010",
                    "10101010",
                    "10101010",
                    "10101010",
                    "10101010",
                    "11111111",
                };

            SetGraphicGlyph('b', crate);

            _state = GameState.IntroScreen;

            Clear(Colors.Black);

            SetFps(6);
        }

        protected override async void DoFrame()
        {
            switch (_state)
            {
                case GameState.IntroScreen:
                    DoIntroScreen();
                    break;

                case GameState.StartGame:
                    StartNewGame();
                    break;

                case GameState.InGame:
                    GameLoop();
                    break;

                case GameState.GameOver:
                    await HandleGameOverAsync();
                    break;
            }
        }

        private void DoIntroScreen()
        {
            var highScoreFirstRow = 4;

            if (!_hasDrawnIntroScreen)
            {
                Clear(Colors.Black);

                // Some instructions
                Paper = Colors.Red;
                Pen = Colors.Yellow;
                this.PrintCentre(2, "* *   S A B O T A G E   * *");

                // Draw high score table
                Pen = Colors.Black;
                Paper = Colors.Blue;
                this.PrintCentre(highScoreFirstRow, "   HIGH SCORES   ");

                highScoreFirstRow += 2;
                Paper = Colors.Black;
                Pen = Colors.White;
                foreach (var highScore in _highScoreManager.HighScores)
                {
                    this.PrintCentre(highScoreFirstRow++, $"{highScore.Name} - {highScore.Score:D5}");
                }

                Paper = Colors.Black;
                Pen = Colors.Cyan;
                this.PrintCentre(13, "Plant a bomb in the warehouse, where it");
                this.PrintCentre(14, "will do as much damage as possible.");
                this.PrintCentre(15, "It's game over if you are shot by a guard");
                this.PrintCentre(16, "or get caught in your own blast.");
                this.PrintCentre(18, "Cursor keys=move, Space=detonate bomb");

                Pen = Colors.Yellow;
                Invert = true;
                this.PrintCentre(20, "SCORING");
                Invert = false;
                this.PrintCentre(21, "10 points for each crate blown up");
                this.PrintCentre(22, "Each guard caught in the blast is worth 200,");
                this.PrintCentre(23, "multiplied by the number of guards");

                Flash = true;
                Pen = Colors.Magenta;
                this.PrintCentre(25, "To start press 1 2 or 3 to select difficulty level");
                Flash = false;

                _hasDrawnIntroScreen = true;
            }
            else
            {
                // Flash the high score in different colours.
                Pen = this.GetRandomColour();
                var topScore = _highScoreManager.HighScores.First();
                this.PrintCentre(highScoreFirstRow + 2, $"{topScore.Name} - {topScore.Score:D5}");
            }

            if (LastKeyPress == Key.D1 || LastKeyPress == Key.D2 || LastKeyPress == Key.D3)
            {
                switch (LastKeyPress)
                {
                    case Key.D1:
                        _numGuards = 1;
                        break;
                    case Key.D2:
                        _numGuards = 2;
                        break;
                    case Key.D3:
                        _numGuards = 3;
                        break;
                }

                _state = GameState.StartGame;
                Clear(Colors.Black);
            }
        }

        private void StartNewGame()
        {
            _map = new bool[ScreenRows, ScreenCols];

            // Draw warehouse crates of different shapes.
            Paper = Colors.RosyBrown;
            Pen = Colors.SaddleBrown;
            // 2x2
            for (var i = 0; i < 250; i++)
            {
                var r = _rnd.Next(1, ScreenRows - 1);
                var c = _rnd.Next(0, ScreenCols - 1);

                Print(r, c, "¬b¬b");
                Print(r + 1, c, "¬b¬b");
                _map[r, c] = true;
                _map[r + 1, c] = true;
                _map[r, c + 1] = true;
                _map[r + 1, c + 1] = true;
            }
            // 2x1
            for (var i = 0; i < 100; i++)
            {
                var r = _rnd.Next(1, ScreenRows);
                var c = _rnd.Next(0, ScreenCols - 1);

                Print(r, c, "¬b¬b");
                _map[r, c] = true;
                _map[r, c + 1] = true;
            }
            // 1x2
            for (var i = 0; i < 100; i++)
            {
                var r = _rnd.Next(1, ScreenRows - 1);
                var c = _rnd.Next(0, ScreenCols);

                Print(r, c, "¬b");
                Print(r + 1, c, "¬b");
                _map[r, c] = true;
                _map[r + 1, c] = true;
            }

            // Row 0 is reserved for scores and messages. There won't be any crates drawn in there, but
            // configure the map to ensure guards don't move into that row.
            for (var c = 0; c < ScreenCols; c++)
            {
                _map[0, c] = true;
            }

            Paper = Colors.Black;

            // Position the player and guard(s) at random locations.
            GenerateRandomCharacterPosition(out var row, out var col);
            _playerRow = row;
            _playerCol = col;

            _guards = new List<SecurityGuard>();
            for (var i = 0; i < _numGuards; i++)
            {
                double distance;
                do
                {
                    GenerateRandomCharacterPosition(out row, out col);

                    // Ensure that the guards aren't positioned too close to the player.
                    distance = Math.Sqrt(Math.Pow(Math.Abs(row - _playerRow), 2) + Math.Pow(Math.Abs(col - _playerCol), 2));
                } 
                while (distance < 15);
                var guard = new SecurityGuard(this, row, col, _map);
                _guards.Add(guard);
            }

            Paper = Colors.Blue;
            Print(0, 0, new string(' ', ScreenCols));
            Pen = Colors.Cyan;
            Print(0, 46, "Score:");

            _frameCount = 0;
            _score = 0;
            _playerKilled = false;
            _bomb = null;
            _state = GameState.InGame;
        }

        /// <summary>
        /// Main game loop, handling player and guard movement, explosions, etc.
        /// </summary>
        private void GameLoop()
        {
            if (_bomb == null || !_bomb.Detonated)
            {
                // Player and guards cannot move once the bomb detonates.
                Paper = Colors.Black;
                Print(_playerRow, _playerCol, " ");

                if (LastKeyPress == Key.Up && _playerRow > 1 && !_map[_playerRow - 1, _playerCol])
                {
                    _playerRow--;
                }
                else if (LastKeyPress == Key.Down && _playerRow < ScreenRows - 1 && !_map[_playerRow + 1, _playerCol])
                {
                    _playerRow++;
                }
                else if (LastKeyPress == Key.Left && _playerCol > 0 && !_map[_playerRow, _playerCol - 1])
                {
                    _playerCol--;
                }
                else if (LastKeyPress == Key.Right && _playerCol < ScreenCols - 1 && !_map[_playerRow, _playerCol + 1])
                {
                    _playerCol++;
                }
                else if (LastKeyPress == Key.Space && _bomb == null)
                {
                    _bomb = new Bomb(
                        this,
                        _map,
                        _playerRow,
                        _playerCol);
                }

                Pen = Colors.Cyan;
                Print(_playerRow, _playerCol, "¬a");

                // Guards move at half the speed of the player
                if (_frameCount % 2 == 0)
                {
                    foreach (var guard in _guards)
                    {
                        guard.Draw(_playerRow, _playerCol);
                    }
                }

                foreach (var guard in _guards)
                {
                    var shot = guard.ShootIfClear(_playerRow, _playerCol);
                    if (shot)
                    {
                        _playerKilled = true;
                        _state = GameState.GameOver;
                        break;
                    }
                }

                _frameCount++;
            }
            
            if (_bomb != null && !_bomb.ExplosionFinished)
            {
                // Draw the bomb
                _bomb.Draw(
                    _playerRow,
                    _playerCol,
                    _guards,
                    ref _score);

                Paper = Colors.Blue;
                Pen = Colors.White;
                Print(0, 53, _score.ToString("D5"));

                if (_bomb.ExplosionFinished)
                {
                    _playerKilled = _bomb.PlayerKilled;
                    _state = GameState.GameOver;
                }
            }
        }

        /// <summary>
        /// Handles the game over state - update of high score table, etc.
        /// </summary>
        private async Task HandleGameOverAsync()
        {
            if (_playerKilled)
            {
                _score = 0;
            }

            Paper = Colors.DarkBlue;
            Pen = this.GetRandomColour();
            this.PrintCentre(10, "G A M E  O V E R");
            this.PrintCentre(11, "=================");
            Pen = Colors.Yellow;
            Print(13, 20, $"Your score is: {_score:D5}");

            Pen = Colors.Cyan;

            if (_highScoreManager.IsHighScore(_score))
            {
                // Capture player initials.
                this.PrintCentre(15, "CONGRATULATIONS! A high score!");
                this.PrintCentre(16, "Enter your initials below:");
                var inits = await InputAsync(18, 28, 3);

                _highScoreManager.AddScore(_score, inits.ToUpper());

                _state = GameState.IntroScreen;
                _hasDrawnIntroScreen = false;
            }
            else
            {
                Print(15, 15, "Press any key to continue ...");

                if (LastKeyPress != Key.None)
                {
                    _state = GameState.IntroScreen;
                    _hasDrawnIntroScreen = false;
                }
            }
        }

        /// <summary>
        /// Generates a random character position that isn't on a crate.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        private void GenerateRandomCharacterPosition(out int row, out int col)
        {
            do
            {
                row = _rnd.Next(ScreenRows - 1) + 1;
                col = _rnd.Next(ScreenCols);
            } while (GetTextCell(row, col).Character != " ");
        }
    }
}
