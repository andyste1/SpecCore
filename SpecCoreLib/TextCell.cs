namespace SpecCoreLib
{
    using System;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// Represents a text cell.
    /// </summary>
    public class TextCell
    {
        private const int Msb = 1 << (SpeccyEngine.CellWidth - 1);

        private readonly WriteableBitmap _wb;
        private readonly GlyphManager _glyphManager;

        private readonly int _x;
        private readonly int _y;

        private Color _pen;
        private Color _paper;
        private bool _invert;
        private bool _flash;
        private char _glyphChar;
        private byte[] _glyphBits;
        private bool _isLocked;

        /// <summary>
        /// Gets or sets the pen colour.
        /// </summary>
        public Color Pen
        {
            get
            {
                return _pen;
            }
            set
            {
                if (_pen != value)
                {
                    _pen = value;
                    Draw();
                }
            }
        }

        /// <summary>
        /// Gets or sets the paper colour.
        /// </summary>
        public Color Paper
        {
            get
            {
                return _paper;
            }
            set
            {
                if (_paper != value)
                {
                    _paper = value;
                    Draw();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the pen and paper are inverted.
        /// </summary>
        public bool Invert
        {
            get
            {
                return _invert;
            }
            set
            {
                if (_invert != value)
                {
                    _invert = value;
                    Draw();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to flash the cell.
        /// </summary>
        public bool Flash
        {
            get
            {
                return _flash;
            }
            set
            {
                if (_flash != value)
                {
                    _flash = value;
                    Draw();
                }
            }
        }

        /// <summary>
        /// Gets the cell's character. Read-only - if you want to set the
        /// character use the SetCharacter() method instead.
        /// </summary>
        /// <remarks>
        /// We've gone with a string here (rather than char) so coders don't need to worry about 
        /// remembering to use apostrophes rather than double quotes.
        /// </remarks>
        public string Character
        {
            get
            {
                return _glyphChar.ToString();
            }
        }

        /// <summary>
        /// Gets a value indicating whether this cell contains a graphic character.
        /// </summary>
        public bool IsGraphicChar { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCell" /> class.
        /// </summary>
        /// <param name="row">The y.</param>
        /// <param name="col">The x.</param>
        /// <param name="wb">The WriteableBitmap.</param>
        /// <param name="glyphManager">The glyph manager.</param>
        internal TextCell(
            int row, 
            int col, 
            WriteableBitmap wb, 
            GlyphManager glyphManager)
        {
            _x = col * SpeccyEngine.CellWidth;
            _y = row * SpeccyEngine.CellHeight;
            _wb = wb;
            _glyphManager = glyphManager;

            SetGlyph(' ', isGraphicChar: false);

            _pen = Colors.Black;
            _paper = Colors.White;
        }

        /// <summary>
        /// Sets the character.
        /// </summary>
        /// <param name="chr">The character.</param>
        /// <param name="isGraphicChar">if set to <c>true</c> [is graphic character].</param>
        /// <remarks>
        /// We've gone with a string here (rather than char) so coders don't need to worry about 
        /// remembering to use apostrophes rather than double quotes.
        /// </remarks>
        public void SetCharacter(Char chr, bool isGraphicChar = false)
        {
            if (_glyphChar != chr) // No point drawing if it's the same character!
            {
                SetGlyph(chr, isGraphicChar);
                Draw();
            }
        }

        /// <summary>
        /// Clears the cell to the given paper colour. The pen colour is reset to Black and
        /// Flash and Invert attributes are turned off.
        /// </summary>
        /// <param name="paper">The paper.</param>
        public void Clear(Color paper)
        {
            _pen = Colors.Black;
            IsGraphicChar = false;
            _paper = paper;
            _invert = false;
            _flash = false;
            SetGlyph(' ', isGraphicChar: false);

            Draw();
        }

        /// <summary>
        /// Draws the specified glyph in the cell, with the specified attributes.
        /// </summary>
        /// <param name="chr">The character to draw.</param>
        /// <param name="isGraphicChar">if set to <c>true</c> [is graphic character].</param>
        /// <param name="pen">The pen.</param>
        /// <param name="paper">The paper.</param>
        /// <param name="invert">if set to <c>true</c> invert the pen and paper colours.</param>
        /// <param name="flash">if set to <c>true</c> [flash].</param>
        internal void Draw(
            char chr,
            bool isGraphicChar,
            Color pen, 
            Color paper, 
            bool invert = false, 
            bool flash = false)
        {
            _pen = pen;
            _paper = paper;
            _invert = invert;
            _flash = flash;

            Draw(chr, isGraphicChar);
        }

        /// <summary>
        /// Draws the specified glyph in the cell using the current cell attributes.
        /// </summary>
        /// <param name="chr">The character to draw.</param>
        /// <param name="isGraphicChar">if set to <c>true</c> [is graphic character].</param>
        internal void Draw(char chr, bool isGraphicChar)
        {
            SetGlyph(chr, isGraphicChar);
            Draw();
        }

        /// <summary>
        /// Copies this cell's attributes and content from another cell.
        /// </summary>
        /// <param name="source">The source cell.</param>
        /// <param name="metaDataOnly">if set to <c>true</c> only copy the metadata. Don't redraw.</param>
        internal void CopyFrom(TextCell source, bool metaDataOnly)
        {
            _paper = source._paper;
            _pen = source._pen;
            _invert = source._invert;
            _flash = source._flash;
            SetGlyph(source._glyphChar, source.IsGraphicChar);

            if (!metaDataOnly)
            {
                Draw();
            }
        }

        /// <summary>
        /// Toggles the flash.
        /// </summary>
        /// <param name="isFlashInvert">if set to <c>true</c> [is flash invert].</param>
        internal void ToggleFlash(bool isFlashInvert)
        {
            if (!_flash)
            {
                return;
            }

            Color drawPen;
            Color drawPaper;

            if (_invert)
            {
                drawPen = isFlashInvert ? _pen : _paper;
                drawPaper = isFlashInvert ? _paper : _pen;
            }
            else
            {
                drawPen = isFlashInvert ? _paper : _pen;
                drawPaper = isFlashInvert ? _pen : _paper;
            }

            Draw(drawPen, drawPaper);
        }

        /// <summary>
        /// Locks this instance.
        /// </summary>
        /// <returns></returns>
        internal CellLocker Lock()
        {
            return new CellLocker(this);
        }

        /// <summary>
        /// Releases a previous lock.
        /// </summary>
        internal void Release()
        {
            if (!_isLocked)
            {
                throw new InvalidOperationException("Not locked");
            }

            _isLocked = false;
            Draw();
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        private void Draw()
        {
            if (_isLocked)
            {
                return;
            }

            var drawPen = _invert ? _paper : _pen;
            var drawPaper = _invert ? _pen : _paper;

            Draw(drawPen, drawPaper);
        }

        /// <summary>
        /// Draws this instsance.
        /// </summary>
        /// <param name="pen">The pen.</param>
        /// <param name="paper">The paper.</param>
        private void Draw(Color pen, Color paper)
        {
            using (_wb.GetBitmapContext()) // Improves performance, effectively batching up changes to the WB then rendering when the using block exits.
            {
                var y = _y;

                for (var r = 0; r < SpeccyEngine.CellHeight; r++)
                {
                    var x = _x;

                    var row = _glyphBits[r];
                    var b = Msb;
                    while (b > 0)
                    {
                        if ((row & b) == b)
                        {
                            _wb.SetPixel(x, y, pen);
                        }
                        else if (paper != Colors.Transparent)
                        {
                            _wb.SetPixel(x, y, paper);
                        }

                        b = b / 2;
                        x++;
                    }

                    y++;
                }
            }
        }

        /// <summary>
        /// Sets the glyph.
        /// </summary>
        /// <param name="chr">The character.</param>
        /// <param name="isGraphicChar">if set to <c>true</c> [is graphic character].</param>
        private void SetGlyph(char chr, bool isGraphicChar)
        {
            if (isGraphicChar)
            {
                _glyphBits = _glyphManager.GetGraphicGlyph(chr);
            }
            else
            {
                _glyphBits = _glyphManager.GetAsciiGlyph(chr);
            }
            _glyphChar = chr;
            IsGraphicChar = isGraphicChar;
        }

        /// <summary>
        /// Used when locking cells.
        /// </summary>
        internal class CellLocker : IDisposable
        {
            private readonly TextCell _cell;

            /// <summary>
            /// Initializes a new instance of the <see cref="CellLocker"/> class.
            /// </summary>
            /// <param name="cell">The cell.</param>
            internal CellLocker(TextCell cell)
            {
                _cell = cell;
                _cell._isLocked = true;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                _cell._isLocked = false;
                _cell.Draw();
            }
        }
    }
}
