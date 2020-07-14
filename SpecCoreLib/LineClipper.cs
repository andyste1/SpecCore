namespace SpecCoreLib
{
    using System.Windows;

    /// <summary>
    /// Cohen-Sutherland line clipper.
    /// </summary>
    internal static class LineClipper
    {
        private const int Inside = 0;
        private const int Left = 1;
        private const int Right = 2;
        private const int Bottom = 4;
        private const int Top = 8;

        /// <summary>
        /// Cohen–Sutherland clipping algorithm clips a line from
        /// P0 = (x0, y0) to P1 = (x1, y1) against a rectangle with
        /// diagonal from (xmin, ymin) to (xmax, ymax).
        /// </summary>
        /// <param name="x0">The x0.</param>
        /// <param name="y0">The y0.</param>
        /// <param name="x1">The x1.</param>
        /// <param name="y1">The y1.</param>
        /// <param name="clipRect">The clipping rectangle.</param>
        /// <returns>
        /// True if successful. The clipped points will be returned in
        /// the original parameters.
        /// </returns>
        internal static bool CohenSutherlandLineClipAndDraw(double x0, double y0, double x1, double y1, Rect clipRect)
        {
            // compute outcodes for P0, P1, and whatever point lies outside the clip rectangle
            var outcode0 = ComputeOutCode(x0, y0, clipRect);
            var outcode1 = ComputeOutCode(x1, y1, clipRect);
            var accept = false;

            while (true)
            {
                if ((outcode0 | outcode1) == 0)
                {
                    // Bitwise OR is 0.
                    accept = true;
                    break;
                }

                if ((outcode0 & outcode1) > 0)
                {
                    // Bitwise AND is not 0.
                    break;
                }

                // Failed both tests, so calculate the line segment to clip
                // from an outside point to an intersection with clip edge
                double x = 0;
                double y = 0;

                // At least one endpoint is outside the clip rectangle; pick it.
                var outcodeOut = outcode0 > 0 ? outcode0 : outcode1;

                // Now find the intersection point;
                // use formulas y = y0 + slope * (x - x0), x = x0 + (1 / slope) * (y - y0)
                if ((outcodeOut & Top) > 0)
                {   
                    // point is above the clip rectangle
                    x = x0 + (x1 - x0) * (clipRect.Bottom - y0) / (y1 - y0);
                    y = clipRect.Bottom;
                }
                else if ((outcodeOut & Bottom) > 0)
                { 
                    // point is below the clip rectangle
                    x = x0 + (x1 - x0) * (clipRect.Top - y0) / (y1 - y0);
                    y = clipRect.Top;
                }
                else if ((outcodeOut & Right) > 0)
                {  
                    // point is to the right of clip rectangle
                    y = y0 + (y1 - y0) * (clipRect.Right - x0) / (x1 - x0);
                    x = clipRect.Right;
                }
                else if ((outcodeOut & Left) > 0)
                {   
                    // point is to the left of clip rectangle
                    y = y0 + (y1 - y0) * (clipRect.Left - x0) / (x1 - x0);
                    x = clipRect.Left;
                }

                // Now we move outside point to intersection point to clip
                // and get ready for next pass.
                if (outcodeOut == outcode0)
                {
                    x0 = x;
                    y0 = y;
                    outcode0 = ComputeOutCode(x0, y0, clipRect);
                }
                else
                {
                    x1 = x;
                    y1 = y;
                    outcode1 = ComputeOutCode(x1, y1, clipRect);
                }
            }

            return accept;
        }

        /// <summary>
        /// Computes the out code.
        /// </summary>
        /// <param name="x">X.</param>
        /// <param name="y">Y.</param>
        /// <param name="clipRect">The clip rect.</param>
        /// <returns></returns>
        private static int ComputeOutCode(double x, double y, Rect clipRect)
        {
            var code = Inside;

            if (x < clipRect.Left)
            {
                // to the left of clip window
                code |= Left;
            }
            else if (x > clipRect.Right)
            {
                // to the right of clip window
                code |= Right;
            }
            if (y < clipRect.Top)
            {
                // below the clip window
                code |= Bottom;
            }
            else if (y > clipRect.Bottom)
            {
                // above the clip window
                code |= Top;
            }

            return code;
        }
    }
}
