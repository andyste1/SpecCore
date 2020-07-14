namespace SpecCoreLib.Demo.SpaceRaidersGame
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using Entities;
    using SpecCoreLib;
    using Utils;

    public class SpaceRaidersGame : SpeccyEngine
    {
        private readonly HighScoreManager _highScoreManager = new HighScoreManager(5);
        private Random _rnd;
        private GameState _state;
        private ForceField _forceField;
        private MotherShip _mothership;
        private int _score;
        private int _lives;
        private PlayerShip _playerShip;

        private enum GameState
        {
            IntroScreen,
            StartGame,
            InGame,
            GameOver,
            PlayerWins
        }

        public SpaceRaidersGame(Window window)
            : base(window)
        {
        }

        protected override void Init()
        {
            _rnd = new Random();
            _state = GameState.IntroScreen;

            Clear(Colors.Black);

            SetFps(10);
        }

        protected override async void DoFrame()
        {
            switch (_state)
            {
                case GameState.IntroScreen:
                    await DoIntroScreenAsync();
                    break;

                case GameState.StartGame:
                    StartNewGame();
                    break;

                case GameState.InGame:
                    await GameLoopAsync();
                    break;

                case GameState.PlayerWins:
                    HandlePlayerWins();
                    break;

                case GameState.GameOver:
                    await HandleGameOverAsync();
                    break;
            }
        }

        private async Task DoIntroScreenAsync()
        {
            // Heading, instructions, random twinkling stars (only draw if not already there)
            var cell = GetTextCell(5, 22);
            if (cell.Character != "-")
            {
                Paper = Colors.Black;
                for (var i = 0; i < 20; i++)
                {
                    Pen = this.GetRandomColour(includeBlack: false);
                    Print(_rnd.Next(40), _rnd.Next(60), "*");
                }

                Flash = true;
                Pen = Colors.White;
                Print(25, 16, " Press any key to start ... ");
                Flash = false;

                Pen = Colors.Cyan;
                Paper = Colors.Red;
                Print(4, 21, "                   ");
                Print(5, 21, " - SPACE RAIDERS - ");
                Print(6, 21, "                   ");
            }

            // Draw high score table
            Pen = Colors.Black;
            Paper = Colors.Magenta;
            Print(9, 22, "   HIGH SCORES   ");

            Paper = Colors.Black;
            var row = 12;
            foreach (var highScore in _highScoreManager.HighScores)
            {
                Pen = row == 12 ? this.GetRandomColour(includeBlack: false) : Colors.Yellow;
                Print(row++, 25, $"{highScore.Name} - {highScore.Score:D5}");
            }

            if (LastKeyPress != Key.None)
            {
                _state = GameState.StartGame;
                await RunOutOfFrameAnimationAsync(() => ScreenFade());
            }
        }

        /// <summary>
        /// Does a screen fade to black.
        /// </summary>
        private void ScreenFade()
        {
            var blankLine = new string(' ', 60);

            Paper = Colors.Blue;
            for (var r = 0; r < 40; r++)
            {
                Print(r, 0, blankLine);
                ForceFrame();
                Thread.Sleep(10);
            }
            Paper = Colors.Black;
            for (var r = 0; r < 40; r++)
            {
                Print(r, 0, blankLine);
                ForceFrame();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        private void StartNewGame()
        {
            // Set up the game screen
            Clear(Colors.Black);

            _mothership = new MotherShip(this, 1);
            _forceField = new ForceField(this, 12);
            _playerShip = new PlayerShip(this, 38);

            _score = 0;
            _lives = 3;

            _state = GameState.InGame;

            // A bit faster will do...
            SetFps(6);
        }

        /// <summary>
        /// The in-game loop.
        /// </summary>
        private async Task GameLoopAsync()
        {
            // Score/lives
            Pen = Colors.Blue;
            Print(0, 1, "Score:");
            Print(0, 51, "Lives:");
            Pen = Colors.Cyan;
            Print(0, 8, _score.ToString().PadLeft(5, '0'));
            Print(0, 58, _lives.ToString());

            _forceField.Draw();
            _mothership.Draw();
            _playerShip.Draw();

            // Ground
            Pen = Colors.Yellow;
            for (var c = 0; c < ScreenCols; c++)
            {
                Print(39, c, "¬K");
            }

            if (LastKeyPress == Key.X)
            {
                _state = GameState.PlayerWins;
                _score = -730;
            }

            // Hit test friendly missile, if one is in flight
            if (_playerShip.Missile != null)
            {
                var hitResult = _forceField.HitTest(_playerShip.Missile);
                if (!hitResult.ProjectileStopped)
                {
                    hitResult = _mothership.HitTest(_playerShip.Missile);
                }

                _score += hitResult.Score;
                if (hitResult.ProjectileStopped)
                {
                    _playerShip.StopMissile();
                }

                if (hitResult.HitEnemyReactor)
                {
                    HandlePlayerWins();
                }
            }

            // Hit test enemy bombs
            foreach (var bomb in _mothership.Bombs)
            {
                var hitResult = _playerShip.HitTest(bomb);
                if (hitResult.PlayerHit)
                {
                    await HandlePlayerDeathAsync();
                    break;
                }
            }

            // ************ test - remove **************
            if (LastKeyPress == Key.C)
            {
                HandlePlayerWins();
            }
        }

        /// <summary>
        /// Handles the player death.
        /// </summary>
        private async Task HandlePlayerDeathAsync()
        {
            var x = _playerShip.Col * 8 + 8;
            var y = 38 * 8;

            // Explosion
            await RunOutOfFrameAnimationAsync(
                () =>
                {
                    for (var i = 0; i < 100; i++)
                    {
                        Pen = this.GetRandomColour(includeBlack: false);
                        Line(x, y, _rnd.Next(ScreenCols * 8), _rnd.Next((ScreenRows - 2) * 8));
                        ForceFrame();
                    }
                });

            _lives--;

            if (_lives == 0)
            {
                _state = GameState.GameOver;
            }

            _mothership.ClearBombs();

            Clear(Colors.Black);
        }

        private void HandlePlayerWins()
        {
            _mothership.Crash();

            _score += 1000;

            _state = GameState.GameOver;
        }

        private async Task HandleGameOverAsync()
        {
            Clear(Colors.Black);
            Pen = Colors.Cyan;
            Print(10, 25, "GAME OVER");
            Print(11, 25, "=========");
            Pen = Colors.Yellow;
            Print(13, 20, $"Your score is: {_score:D5}");

            Pen = Colors.Magenta;

            if (_highScoreManager.IsHighScore(_score))
            {
                // Capture player initials.
                Print(15, 10, "CONGRATULATIONS! You have a high score!");
                Print(17, 17, "Enter your initials below:");
                var inits = await InputAsync(18, 28, 3);

                _highScoreManager.AddScore(_score, inits.ToUpper());

                _state = GameState.IntroScreen;

                await RunOutOfFrameAnimationAsync(() => ScreenFade());
            }
            else
            {
                Print(15, 15, "Press any key to continue ...");

                if (LastKeyPress != Key.None)
                {
                    _state = GameState.IntroScreen;

                    await RunOutOfFrameAnimationAsync(() => ScreenFade());
                }
            }
        }
    }
}