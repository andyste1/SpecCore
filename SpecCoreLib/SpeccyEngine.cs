// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global
namespace SpecCoreLib
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Effects;
    using System.Windows.Media.Imaging;
    using DispatcherPriority = System.Windows.Threading.DispatcherPriority;

    /// <summary>
    /// Main class
    /// </summary>
    public class SpeccyEngine
    {
        public const int ScreenCols = 60;
        public const int ScreenRows = 40;
        internal const int CellWidth = 8;
        internal const int CellHeight = 8;

        private const int FlashIntervalMs = 500;
        private const int DefaultFps = 4;

        private readonly GlyphManager _glyphManager = new GlyphManager();
        private readonly Rect _clipRect;
        private readonly Timer _flashTimer;
        private readonly InputHandler _inputHandler;

        private Image _imageCtrl;
        private WriteableBitmap _wb;
        private TextCell[,] _cells; // [row,col]
        private bool _isFlashInvert = true;
        private volatile bool _isWaitingForUserInput;
        private volatile bool _frameUpdating;
        private int _zoom = 2;

        /// <summary>
        /// The list of standard ZX Spectrum colours
        /// </summary>
        public Color[] SpectrumColours = new[]
        {
            Colors.Black,
            Colors.Blue,
            Colors.Red,
            Colors.Magenta,
            Colors.Lime,
            Colors.Cyan,
            Colors.Yellow,
            Colors.White
        };

        /// <summary>
        /// Gets or sets the pen colour for subsequent text and drawing.
        /// </summary>
        public Color Pen { get; set; }

        /// <summary>
        /// Gets or sets the paper colour for subsequent text.
        /// </summary>
        public Color Paper { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subsequent printed text should be inverted.
        /// </summary>
        public bool Invert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether subsequent printed text should flash.
        /// </summary>
        public bool Flash { get; set; }

        /// <summary>
        /// Gets the most recent key press.
        /// </summary>
        public Key LastKeyPress { get; private set; }

        internal Window Window { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpeccyEngine"/> class.
        /// </summary>
        protected SpeccyEngine(Window window)
        {
            Window = window;
            Window.Initialized += WindowOnInitialized;

            _clipRect = new Rect(
                0,
                0,
                (ScreenCols * CellWidth) - 1,
                (ScreenRows * CellHeight) - 1);

            _inputHandler = new InputHandler(this);

            _flashTimer = new Timer(FlashCallback, null, FlashIntervalMs, FlashIntervalMs);

            SetFps(DefaultFps);

            LastKeyPress = Key.None;
        }

        /// <summary>
        /// Gets the attributes of the specified text cell.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns></returns>
        public TextCell GetTextCell(int row, int column)
        {
            if (row < 0 || row > ScreenRows - 1 || column < 0 || column > ScreenCols - 1)
            {
                return null;
            }

            return _cells[row, column];
        }

        /// <summary>
        /// Gets the colour of the specified pixel.
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns></returns>
        public Color GetPixel(int x, int y)
        {
            return !IsPointWithinScreen(x, y)
                ? Colors.Black
                : _wb.GetPixel(x, y);
        }

        /// <summary>
        /// Sets the frame rate.
        /// </summary>
        /// <param name="fps">The FPS.</param>
        public void SetFps(int fps)
        {
            ThrottledCompositionTarget.Fps = fps;
        }

        /// <summary>
        /// Restores the frame rate to the default value.
        /// </summary>
        public void RestoreFps()
        {
            ThrottledCompositionTarget.Fps = DefaultFps;
        }

        /// <summary>
        /// Clears the screen.
        /// </summary>
        /// <param name="paper">The paper color.</param>
        public void Clear(Color paper)
        {
            _wb.Clear(paper);

            for (var r = 0; r < ScreenRows; r++)
            {
                for (var c = 0; c < ScreenCols; c++)
                {
                    _cells[r, c].Clear(paper);
                }
            }
        }

        /// <summary>
        /// Scrolls all text up the screen by one line, then clears the last line.
        /// </summary>
        public void Scroll()
        {
            var numRowsToCopy = ScreenRows - 1;

            // Copy the screen data. Blit is used to ensure that any shapes (lines, points, circles, etc) are
            // also scrolled.
            var rectSize = new Size(ScreenCols * CellWidth, numRowsToCopy * CellHeight);
            var sourceRect = new Rect(new Point(0, CellHeight), rectSize);
            var destRect = new Rect(new Point(0, 0), rectSize);
            _wb.Blit(destRect, _wb, sourceRect, WriteableBitmapExtensions.BlendMode.None);

            // The text cells' metadata needs to be copied so that we know which chars, colours, etc. are where.
            // It's not necessary to redraw though, as the above Blit() has already done this.
            for (var r = 0; r < numRowsToCopy; r++)
            {
                for (var c = 0; c < ScreenCols; c++)
                {
                    _cells[r, c].CopyFrom(_cells[r + 1, c], metaDataOnly: true);
                }
            }

            // Clear the bottom line.
            for (var c = 0; c < ScreenCols; c++)
            {
                _cells[ScreenRows - 1, c].Clear(Paper);
            }
        }

        /// <summary>
        /// Produces a beep sound.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="durationMs">The duration ms.</param>
        public void Beep(int frequency, int durationMs)
        {
            Task.Run(() => Console.Beep(frequency, durationMs));
        }

        /// <summary>
        /// Prints some text.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        /// <param name="text">The text.</param>
        /// <returns>
        /// The column of the last character in the text.
        /// </returns>
        public int Print(int row, int col, string text)
        {
            const char GraphicEsc = '¬';

            var r = row;
            var c = col;

            if (r < 0 || r >= ScreenRows)
            {
                return c;
            }

            if (string.IsNullOrEmpty(text))
            {
                return c;
            }

            var isGraphicChar = false;
            foreach (var chr in text)
            {
                if (chr == GraphicEsc)
                {
                    isGraphicChar = true;
                    continue;
                }

                if (c >= 0 && c < ScreenCols)
                {
                    _cells[r, c].Draw(chr, isGraphicChar, Pen, Paper, Invert, Flash);
                }

                isGraphicChar = false;

                c++;
            }

            return c - 1;
        }

        ///// <summary>
        ///// Determines whether the specified key is pressed.
        ///// </summary>
        ///// <param name="key">The key.</param>
        ///// <returns></returns>
        //protected bool IsKeyDown(Key key)
        //{
        //    return Keyboard.IsKeyDown(key);
        //}

        /// <summary>
        /// Plot a point in the current pen colour.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        public void Plot(int x, int y)
        {
            if (!IsPointWithinScreen(x, y))
            {
                return;
            }

            _wb.SetPixel(x, y, Pen);
        }

        /// <summary>
        /// Draws a line in the current pen colour.
        /// </summary>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="x2">The x2.</param>
        /// <param name="y2">The y2.</param>
        public void Line(int x1, int y1, int x2, int y2)
        {
            var sw = new Stopwatch();
            sw.Start();

            _wb.DrawLine(x1, y1, x2, y2, Pen);
            //if (LineClipper.CohenSutherlandLineClipAndDraw(x1, y1, x2, y2, _clipRect))
            //{
            //    _wb.DrawLine(x1, y1, x2, y2, Pen);
            //}

            Debug.WriteLine("Line: " + sw.ElapsedMilliseconds);
        }

        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="x">The centre X.</param>
        /// <param name="y">The centre Y.</param>
        /// <param name="radiusX">The X radius.</param>
        /// <param name="radiusY">The Y radius.</param>
        public void Ellipse(int x, int y, int radiusX, int radiusY)
        {
            var colour = WriteableBitmapExtensions.ConvertColor(Pen);
            _wb.DrawEllipseCenteredClipped(x, y, radiusX, radiusY, colour, out _);
        }

        /// <summary>
        /// Draw an ellipse.
        /// </summary>
        /// <param name="x">The centre X.</param>
        /// <param name="y">The centre Y.</param>
        /// <param name="radius">The radius.</param>
        public void Circle(int x, int y, int radius)
        {
            var colour = WriteableBitmapExtensions.ConvertColor(Pen);
            _wb.DrawEllipseCenteredClipped(x, y, radius, radius, colour, out _);
        }

        /// <summary>
        /// Prompt for input. This method call must be prefixed with the "await" keyword.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="maxLength">The maximum length of the input.</param>
        /// <returns></returns>
        public async Task<string> InputAsync(
            int row,
            int column,
            int maxLength = InputHandler.MaxMaxLength)
        {
            _isWaitingForUserInput = true;

            try
            {
                return await _inputHandler.InputAsync(row, column, maxLength);
            }
            finally
            {
                _isWaitingForUserInput = false;
                LastKeyPress = Key.None; // Ensure the "Enter" press used to submit the input isn't exposed as the last keypress.
            }
        }

        /// <summary>
        /// Sets a graphic glyph.
        /// </summary>
        /// <param name="chr">The character.</param>
        /// <param name="data">The character data.</param>
        public void SetGraphicGlyph(char chr, string[] data)
        {
            _glyphManager.SetGraphicGlyph(chr, data);
        }

        /// <summary>
        /// Allows game code to run an ad-hoc animation outside the normal game loop.
        /// The game loop will be paused while the given delegate is executed. Any graphics methods called from within
        /// the action delegate (e.g. after each iteration of a "for" loop) should be followed up with a call to
        /// ForceFrame(), allowing the main thread to update the UI with those changes.
        /// </summary>
        /// <param name="action">The action.</param>
        public async Task RunOutOfFrameAnimationAsync(Action action)
        {
            ThrottledCompositionTarget.FrameUpdating -= OnFrame;

            await Window.Dispatcher.BeginInvoke(action);

            ThrottledCompositionTarget.FrameUpdating += OnFrame;
        }

        /// <summary>
        /// Forces the UI to update. Typically used within a delegate passed to the RunOutOfFrameAnimation() method.
        /// </summary>
        public void ForceFrame()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate
                {
                }));
            }
            catch
            {
                // Dispatcher can become invalidated during shutdown
            }
        }

        private void WindowOnInitialized(object sender, EventArgs e)
        {
            Window.SizeToContent = SizeToContent.WidthAndHeight;

            Window.KeyDown += OnKeyDown;

            Pen = Colors.Black;
            Paper = Colors.White;

            _imageCtrl = new Image
                {
                    Width = ScreenCols * CellWidth * _zoom,
                    Height = ScreenRows * CellHeight * _zoom,
                };

            _wb = BitmapFactory.New(CellWidth * ScreenCols, CellWidth * ScreenRows);
            _wb.Clear(Paper);

            RenderOptions.SetBitmapScalingMode(_imageCtrl, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(_imageCtrl, EdgeMode.Aliased);

            _imageCtrl.Source = _wb;

            Window.Content = _imageCtrl;

            _cells = new TextCell[ScreenRows, ScreenCols];
            for (var r = 0; r < ScreenRows; r++)
            {
                for (var c = 0; c < ScreenCols; c++)
                {
                    _cells[r, c] = new TextCell(r, c, _wb, _glyphManager);
                }
            }

            Init();

            ThrottledCompositionTarget.FrameUpdating += OnFrame;
        }

        /// <summary>
        /// Initialization code
        /// </summary>
        protected virtual void Init()
        {
        }

        /// <summary>
        /// Frame code.
        /// </summary>
        protected virtual void DoFrame()
        {
        }

        /// <summary>
        /// Prints text followed by a cursor.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="col">The col.</param>
        /// <param name="text">The text.</param>
        /// <returns>The cursor's column.</returns>
        internal int PrintWithCursor(int row, int col, string text)
        {
            var lastCol = Print(row, col, text);

            if (!string.IsNullOrEmpty(text))
            {
                lastCol++;
            }

            // Flashing cursor at end of text.
            Flash = true;
            Print(row, lastCol, " ");
            Flash = false;

            return lastCol;
        }

        /// <summary>
        /// key down event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F6 && _zoom > 1)
            {
                _zoom--;
                _imageCtrl.Width = ScreenCols * CellWidth * _zoom;
                _imageCtrl.Height = ScreenRows * CellHeight * _zoom;
            }
            if (e.Key == Key.F7 && _zoom < 4)
            {
                _zoom++;
                _imageCtrl.Width = ScreenCols * CellWidth * _zoom;
                _imageCtrl.Height = ScreenRows * CellHeight * _zoom;
            }

            LastKeyPress = e.Key;
        }

        /// <summary>
        /// Raises the Frame event.
        /// </summary>
        private void OnFrame(object sender, EventArgs e)
        {
            if (!_frameUpdating && !_isWaitingForUserInput)
            {
                try
                {
                    _frameUpdating = true;
                    DoFrame();

                    LastKeyPress = Key.None;
                }
                finally
                {
                    _frameUpdating = false;
                }
            }
        }

        /// <summary>
        /// Determines whether the given point is within the screen bounds.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns></returns>
        private bool IsPointWithinScreen(int x, int y)
        {
            return _clipRect.Contains(x, y);
        }

        /// <summary>
        /// Flash timer callback.
        /// </summary>
        /// <param name="state">The state.</param>
        private void FlashCallback(object state)
        {
            try
            {
                Window.Dispatcher?.Invoke(() =>
                {
                    for (var r = 0; r < ScreenRows; r++)
                    {
                        for (var c = 0; c < ScreenCols; c++)
                        {
                            _cells[r, c].ToggleFlash(_isFlashInvert);
                        }
                    }

                    _isFlashInvert = !_isFlashInvert;
                });
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
