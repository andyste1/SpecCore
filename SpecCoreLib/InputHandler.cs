namespace SpecCoreLib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    /// <summary>
    /// Handles user input. Only supports standard keyboard characters, carriage return and backspace.
    /// </summary>
    internal class InputHandler
    {
        internal const int MaxMaxLength = SpeccyEngine.ScreenCols - 1;

        private readonly SpeccyEngine _speccy;

        private SemaphoreSlim _semaphoreSignal;
        private string _userInput;
        private int _startRow;
        private int _startColumn;
        private int _cursorColumn;
        private int _maxLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputHandler" /> class.
        /// </summary>
        /// <param name="speccy">The spec window.</param>
        public InputHandler(SpeccyEngine speccy)
        {
            _speccy = speccy;
        }

        /// <summary>
        /// Prompts the user for input.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <param name="maxLength">The maximum length of the input.</param>
        /// <returns></returns>
        internal async Task<string> InputAsync(int row, int column, int maxLength = MaxMaxLength)
        {
            _userInput = string.Empty;

            _startRow = row;
            _startColumn = column;
            _cursorColumn = column;
            _maxLength = Math.Min(MaxMaxLength, maxLength);

            try
            {
                _speccy.Window.TextInput += OnTextInput;

                UpdateScreen();

                _semaphoreSignal = new SemaphoreSlim(0, 1);
                await _semaphoreSignal.WaitAsync();
            }
            finally
            {
                _speccy.Window.TextInput -= OnTextInput;
            }

            return _userInput;
        }

        /// <summary>
        /// Handles text input event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> instance containing the event data.</param>
        private void OnTextInput(object sender, TextCompositionEventArgs e)
        {
            const string Backspace = "\b";
            const string Return = "\r";

            if (e.Text == Return)
            {
                // Return - finished. Clear the input area of text and cursor.
                for (var col = _startColumn; col <= _cursorColumn; col++)
                {
                    var cell = _speccy.GetTextCell(_startRow, col);
                    using (var locker = cell.Lock())
                    {
                        cell.SetCharacter(' ');
                        cell.Flash = false;
                    }
                }

                _semaphoreSignal.Release();
                return;
            }

            if (e.Text == Backspace)
            {
                // Backspace
                if (_userInput.Length > 0)
                {
                    _userInput = _userInput.Substring(0, _userInput.Length - 1);
                    UpdateScreen();
                }
                return;
            }

            if (_userInput.Length >= _maxLength)
            {
                return;
            }

            _userInput += e.Text;
            _cursorColumn++;
            UpdateScreen();
        }

        /// <summary>
        /// Updates the screen.
        /// </summary>
        private void UpdateScreen()
        {
            Application.Current.Dispatcher.Invoke(() =>
                {
                    // Clear current cursor, if any.
                    var cursorCell = _speccy.GetTextCell(_startRow, _cursorColumn);
                    cursorCell.Flash = false;

                    // Print the user input followed by a cursor.
                    _cursorColumn = _speccy.PrintWithCursor(_startRow, _startColumn, _userInput);
                });
        }
    }
}
