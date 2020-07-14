using System;

namespace SpecCoreLib.Demo.Utils
{
    using System.Windows.Media;

    public static class SpeccyEngineExtensions
    {
        /// <summary>
        /// Prints a line of text centred on the screen.
        /// </summary>
        /// <param name="se">The se.</param>
        /// <param name="row">The row.</param>
        /// <param name="text">The text.</param>
        public static void PrintCentre(this SpeccyEngine se, int row, string text)
        {
            var col = (SpeccyEngine.ScreenCols - text.Length) / 2;
            se.Print(row, col, text);
        }

        /// <summary>
        /// Gets a random colour.
        /// </summary>
        /// <param name="se">The se.</param>
        /// <param name="includeBlack">If false, this will never return Black.</param>
        /// <returns></returns>
        public static Color GetRandomColour(this SpeccyEngine se, bool includeBlack = false)
        {
            Color colour;

            var rnd = new Random();

            do
            {
                colour = se.SpectrumColours[rnd.Next(se.SpectrumColours.Length)];
            } while (colour == Colors.Black);

            return colour;
        }
    }
}
