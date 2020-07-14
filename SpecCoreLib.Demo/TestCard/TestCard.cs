namespace SpecCoreLib.Demo.TestCard
{
    using System;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    /// <summary>
    /// Simple program that demonstrates many of the features provided by the engine.
    /// </summary>
    /// <seealso cref="SpecCoreLib.SpeccyEngine" />
    public class TestCard : SpeccyEngine
    {
        private bool _capturedName;
        private int _playerCol;
        private int _playerRow;

        public TestCard(Window window)
            : base(window)
        {
        }

        protected override void Init()
        {
            // Clear the screen
            Clear(Colors.Black);

            Paper = Colors.Black;
            Pen = Colors.White;

            // Printing
            Print(0, 0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");
            Print(1, 0, "abcdefghijklmnopqrstuvwxyz");
            Print(2, 0, "0123456789");
            Print(3, 0, "¬A¬B¬C¬D¬E¬F¬G¬H¬I¬J¬K¬L¬M¬N¬O¬P¬Q¬R¬S¬T¬U¬V¬W¬X");

            Invert = true;
            Print(4, 0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            Invert = false;
            Flash = true;
            Print(5, 0, "I'm flashing!");
            Flash = false;

            // Shapes
            Pen = Colors.Magenta;
            Line(300, 20, 450, 60);
            Pen = Colors.Lime;
            Circle(350, 100, 40);
            Pen = Colors.Blue;
            Ellipse(350, 150, 75, 40);

            // User-defined graphics
            var missile = new[]
                {
                    "00111100",
                    "01000010",
                    "10100101",
                    "10000001",
                    "10100101",
                    "10011001",
                    "01000010",
                    "00111100",
                };
            SetGraphicGlyph('a', missile);

            Pen = Colors.DarkOrange;
            Print(29, 30, "User-defined graphics:");
            Print(31, 30, "¬a ¬a ¬a ¬a ¬a ¬a ¬a ¬a ¬a ¬a");

            _capturedName = false;
        }

        protected override async void DoFrame()
        {
            // Capture the user's name if not already done so.
            if (!_capturedName)
            {
                Pen = Colors.Cyan;
                Paper = Colors.Blue;
                Print(7, 0, "Enter your name (max length 10):");

                Paper = Colors.Black;
                Pen = Colors.White;
                var name = await InputAsync(8, 0, 10);

                Pen = Colors.Red;
                Paper = Colors.Yellow;
                Print(7, 0, $"You entered: {name}                                     ");

                Paper = Colors.Black;
                Pen = Colors.Cyan;
                Print(9, 0, "Use cursor keys to control asterisk: ");
                Print(10, 0, "Press '1' to see an 'out of frame' animation");

                _capturedName = true;

                _playerRow = 15;
                _playerCol = 15;
            }

            // Use the cursor keys to move an asterisk around the screen
            Paper = Colors.Black;
            Print(_playerRow, _playerCol, " ");

            if (LastKeyPress == Key.Up && _playerRow > 0)
            {
                Beep(1000, 50);
                _playerRow--;
            }
            else if (LastKeyPress == Key.Down && _playerRow < 39)
            {
                Beep(1500, 50);
                _playerRow++;
            }
            else if (LastKeyPress == Key.Left && _playerCol > 0)
            {
                Beep(2000, 50);
                _playerCol--;
            }
            else if (LastKeyPress == Key.Right && _playerCol < 59)
            {
                Beep(2500, 50);
                _playerCol++;
            }
            else if (LastKeyPress == Key.D1)
            {
                await RunOutOfFrameAnimationAsync(() => DrawStarburst());
            }

            Paper = Colors.Yellow;
            Pen = Colors.Black;
            Print(_playerRow, _playerCol, "*");
        }

        private void DrawStarburst()
        {
            var rnd = new Random();
            var x1 = rnd.Next(SpeccyEngine.ScreenCols * 8 - 1);
            var y1 = rnd.Next(SpeccyEngine.ScreenRows * 8 - 1);

            for (var i = 0; i < 150; i++)
            {
                var x2 = rnd.Next(160) - 80;
                var y2 = rnd.Next(160) - 80;

                Pen = GetRandomColour(rnd);
                Line(x1, y1, x1 + x2, y1 + y2);

                ForceFrame();
                Thread.Sleep(10);
            }
        }

        /// <summary>
        /// Gets a random colour.
        /// </summary>
        /// <returns></returns>
        private Color GetRandomColour(Random rnd)
        {
            return SpectrumColours[rnd.Next(SpectrumColours.Length)];
        }
    }
}
