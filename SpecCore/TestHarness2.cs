namespace SpecCore
{
    using System;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using SpecCoreLib;

    public class HelloWorld : SpeccyEngine
    {
        public HelloWorld(Window window)
            : base(window)
        {
        }

        protected override void Init()
        {
            Clear(Colors.White);
            Paper = Colors.White;
            Pen = Colors.Black;

            var row = 1;
            var col = 5;
            var rowCount = 0;
            var chr = 'A';
            while (chr <= 'X')
            {
                Print(row, col, $"{chr} ¬{chr}");

                chr++;
                col += 7;

                rowCount++;
                if (rowCount >= 6)
                {
                    rowCount = 0;
                    row += 2;
                    col = 5;
                }
            }

            var missile = new[]
                {
                    "00011000",
                    "00011000",
                    "00111100",
                    "00011000",
                    "00011000",
                    "00011000",
                    "00111100",
                    "01100110",
                };
            SetGraphicGlyph('a', missile);

            Print(12, 5, "¬a");
        }

        protected override void DoFrame()
        {
        }
    }




    public class TestHarness2 : SpeccyEngine
    {
        private readonly Stopwatch _sw = new Stopwatch();

        private int _playerCol;
        private int _playerRow;
        private int _robotCol = 0;
        private int _robotRow = 30;
        private bool _needInput = true;
        private string _userName;

        public TestHarness2(Window window)
            : base(window)
        {
        }

        /// <summary>
        /// Initialization code
        /// </summary>
        protected override void Init()
        {
            // Printing test
            Print(0, 0, "abca");
            Pen = Colors.Magenta;
            Print(1, 0, "abca");
            Paper = Colors.Yellow;
            Pen = Colors.Blue;
            Flash = true;
            Print(2, 0, "abca");
            Flash = false;

            Invert = true;
            Paper = Colors.White;
            Pen = Colors.Black;

            Print(3, 0, "abc");
            Pen = Colors.Magenta;
            Print(4, 0, "abc");
            Paper = Colors.Yellow;
            Pen = Colors.Blue;
            Flash = true;
            Print(5, 0, "abc");
            Flash = false;

            Invert = false;
            Pen = Colors.Red;
            Print(6, 50, "abcabcabcabcabcabcabc");
            Pen = Colors.Cyan;
            Print(8, -10, "abcabcabcabcabc");

            Paper = Colors.White;
            Pen = Colors.Black;
            Print(9, 0, "abc?abc?abc?");

            var r = new Random();

            // Pixels
            for (var i = 0; i < 500; i++)
            {
                Pen = Color.FromRgb((byte)r.Next(255), (byte)r.Next(255), (byte)r.Next(255));
                Plot(r.Next(480), r.Next(320));
            }

            // Overlaying text
            Pen = Colors.Red;
            Paper = Colors.Transparent;
            Print(11, 30, "-");
            Print(11, 30, "|");

            // Lines
            Line(10, 10, 300, 300);
            Pen = Colors.LimeGreen;
            Line(-50, 50, 300, 300);
            Line(-30, -30, 200, -10);

            Pen = Colors.MediumPurple;
            Ellipse(150, 150, 60, 60);
            Ellipse(450, 20, 60, 60);

            // Random chars with transparent background
            Pen = Colors.Gold;
            Paper = Colors.Transparent;
            for (var i = 0; i < 200; i++)
            {
                Print(r.Next(40), r.Next(60), "!");
            }

            // All ASCII
            Pen = Colors.Black;
            Paper = Colors.White;
            var chars = "";
            for (var i = 32; i < 82; i++)
            {
                chars += Convert.ToChar(i);
            }
            Print(25, 0, chars);
            chars = "";
            for (var i = 82; i < 127; i++)
            {
                chars += Convert.ToChar(i);
            }
            Print(26, 0, chars);

            // Built-in graphics characters
            Print(27, 0, "¬A¬B¬C¬D¬E¬F¬G¬H¬I¬J¬K¬L¬M¬N¬O¬P¬Q¬R¬S¬T¬U¬V¬W¬X");

            // User-defined graphic
            var missile = new[]
                      {
                          "00011000",
                          "00011000",
                          "00111100",
                          "01011010",
                          "00011000",
                          "00011000",
                          "00111100",
                          "01111110",
                      };
            SetGraphicGlyph('a', missile);

            Pen = Colors.DarkOrange;
            Print(28, 0, "¬a¬a¬a¬a¬a¬a¬a¬a¬a¬a");

            Pen = Colors.DeepPink;
            Line(479, 0, 479, 319);
            Line(0, 319, 479, 319);
        }

        /// <summary>
        /// Frame code.
        /// </summary>
        protected override async void DoFrame()
        {
            if (_needInput)
            {
                // First time in, prompt the user for input
                // Put a border around the input field.
                Paper = Colors.Blue;
                Print(20, 20, "            ");
                Print(21, 20, " ");
                Print(21, 31, " ");
                Print(22, 20, "            ");
                Paper = Colors.Gold;
                Print(21, 21, "          ");

                // Input
                Pen = Colors.Black;
                _userName = await InputAsync(21, 21, 10);
                Debug.WriteLine("Input: " + _userName);
                _needInput = false;

                // Display what the user entered...
                Pen = Colors.White;
                Paper = Colors.DeepPink;
                Print(25, 20, "User input: " + _userName);
                Paper = Colors.White;
                Pen = Colors.Black;

                Debug.WriteLine("After input");

                // Clear the screen:-
                //Clear(Colors.Yellow);
            }

            _sw.Restart();

            // Sprite movement
            Print(_playerRow, _playerCol, " ");
            Print(_robotRow, _robotCol, " ");

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
            else if (LastKeyPress == Key.S)
            {
                Scroll(); // Test scrolling
            }

            _robotCol++;
            if (_robotCol == 60)
            {
                _robotCol = 0;
                _robotRow++;
                if (_robotRow == 40)
                {
                    _robotRow = 0;
                }
            }

            Print(_playerRow, _playerCol, "¬V");
            Print(_robotRow, _robotCol, "@");

            if (_playerRow == _robotRow && _playerCol == _robotCol)
            {
                Clear(Colors.Blue);
                Pen = Colors.Yellow;
                Paper = Colors.Red;
                Print(0, 10, $"******* {_userName} - YOU GOT ME! *******");
                Pen = Colors.Black;
                Paper = Colors.White;
            }

            // Test scrolling
            //Scroll();

            // Test speed of many screen updates
            // 100 chars = 4ms; 400=16ms; full screen 60x40 (2400) = approx 360ms
            //Pen = _colourFlipFlop ? Colors.Red : Colors.Orange;
            //_colourFlipFlop = !_colourFlipFlop;
            //for (var r = 0; r <= 40; r++)
            //{
            //    for (var c = 0; c <= 60; c++)
            //    {
            //        Print(r, c, "X");
            //    }
            //}
            //Pen = Colors.Black;

            Debug.WriteLine($"DoFrame() duration: {_sw.ElapsedMilliseconds}ms");
        }
    }
}
