namespace SpecCoreLib
{
    using System.Collections.Generic;
    using System.Windows.Media.Imaging;

    /// <summary>
    /// WriteableBitmap extension methods (over and above WriteableBitmapEx).
    /// </summary>
    internal static class WriteableBitmapLocalExtensions
    {
        /// <summary>
        /// A Fast Bresenham Type Algorithm For Drawing Ellipses http://homepage.smc.edu/kennedy_john/belipse.pdf
        /// Uses a different parameter representation than DrawEllipse().
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <param name="xc">The x-coordinate of the ellipses center.</param>
        /// <param name="yc">The y-coordinate of the ellipses center.</param>
        /// <param name="xr">The radius of the ellipse in x-direction.</param>
        /// <param name="yr">The radius of the ellipse in y-direction.</param>
        /// <param name="color">The color for the line.</param>
        /// <param name="writtenPixels">A list of the pixel coordinates that were written to.</param>
        public static unsafe void DrawEllipseCenteredClipped(
            this WriteableBitmap bmp, 
            int xc, 
            int yc, 
            int xr, 
            int yr, 
            int color,
            out List<XyCoordinate> writtenPixels)
        {
            writtenPixels = new List<XyCoordinate>();

            // This "using" batches up all the changes and only renders upon exit. Provides a huge performance boost.
            using (var context = bmp.GetBitmapContext())
            {
                var pixels = context.Pixels;
                var w = context.Width;
                var h = context.Height;

                // Avoid endless loop
                if (xr < 1 || yr < 1)
                {
                    return;
                }

                // Init vars
                int uh, lh, uy, ly, lx, rx;
                int x = xr;
                int y = 0;
                int xrsqtwo = (xr * xr) << 1;
                int yrsqtwo = (yr * yr) << 1;
                int xchg = yr * yr * (1 - (xr << 1));
                int ychg = xr * xr;
                int err = 0;
                int xstopping = yrsqtwo * xr;
                int ystopping = 0;
                bool clipUy = false;
                bool clipLy = false;
                bool clipRx = false;
                bool clipLx = false;

                // Draw first set of points counter clockwise where tangent line slope > -1.
                while (xstopping >= ystopping)
                {
                    clipUy = false;
                    clipLy = false;
                    clipRx = false;
                    clipLx = false;

                    // Draw 4 quadrant points at once
                    uy = yc + y;                  // Upper half
                    ly = yc - y;                  // Lower half
                    
                    if (uy < 0)
                    {
                        clipUy = true;
                        uy = 0;
                    }
                    if (uy >= h) 
                    {
                        clipUy = true;
                        uy = h - 1;
                    }
                    if (ly < 0)
                    {
                        clipLy = true;
                        ly = 0;
                    }
                    if (ly >= h)
                    {
                        clipLy = true;
                        ly = h - 1;
                    }

                    uh = uy * w;                  // Upper half
                    lh = ly * w;                  // Lower half

                    rx = xc + x;
                    lx = xc - x;

                    if (rx < 0)
                    {
                        clipRx = true;
                        rx = 0;
                    }
                    if (rx >= w)
                    {
                        clipRx = true;
                        rx = w - 1;
                    }
                    if (lx < 0)
                    {
                        clipLx = true;
                        lx = 0;
                    }
                    if (lx >= w)
                    {
                        clipLx = true;
                        lx = w - 1;
                    }

                    if (!clipRx && !clipUy)
                    {
                        pixels[rx + uh] = color; // Quadrant I (Actually an octant)
                        writtenPixels.Add(new XyCoordinate(rx, uy));
                    }
                    if (!clipLx && !clipUy)
                    {
                        pixels[lx + uh] = color; // Quadrant II
                        writtenPixels.Add(new XyCoordinate(lx, uy));
                    }
                    if (!clipLx && !clipLy)
                    {
                        pixels[lx + lh] = color; // Quadrant III
                        writtenPixels.Add(new XyCoordinate(lx, ly));
                    }
                    if (!clipRx && !clipLy)
                    {
                        pixels[rx + lh] = color; // Quadrant IV
                        writtenPixels.Add(new XyCoordinate(rx, ly));
                    }

                    y++;
                    ystopping += xrsqtwo;
                    err += ychg;
                    ychg += xrsqtwo;
                    if ((xchg + (err << 1)) > 0)
                    {
                        x--;
                        xstopping -= yrsqtwo;
                        err += xchg;
                        xchg += yrsqtwo;
                    }
                }

                // ReInit vars
                x = 0;
                y = yr;
                uy = yc + y;                  // Upper half
                ly = yc - y;                  // Lower half

                if (uy < 0)
                {
                    clipUy = true;
                    uy = 0;
                }
                if (uy >= h)
                {
                    clipUy = true;
                    uy = h - 1;
                }
                if (ly < 0)
                {
                    clipLy = true;
                    ly = 0;
                }
                if (ly >= h)
                {
                    clipLy = true;
                    ly = h - 1;
                }

                uh = uy * w;                  // Upper half
                lh = ly * w;                  // Lower half
                xchg = yr * yr;
                ychg = xr * xr * (1 - (yr << 1));
                err = 0;
                xstopping = 0;
                ystopping = xrsqtwo * yr;

                // Draw second set of points clockwise where tangent line slope < -1.
                while (xstopping <= ystopping)
                {
                    clipRx = false;
                    clipLx = false;

                    // Draw 4 quadrant points at once
                    rx = xc + x;
                    lx = xc - x;

                    if (rx < 0)
                    {
                        clipRx = true;
                        rx = 0;
                    }
                    if (rx >= w)
                    {
                        clipRx = true;
                        rx = w - 1;
                    }
                    if (lx < 0)
                    {
                        clipLx = true;
                        lx = 0;
                    }
                    if (lx >= w)
                    {
                        clipLx = true;
                        lx = w - 1;
                    }

                    if (!clipRx && !clipUy)
                    {
                        pixels[rx + uh] = color; // Quadrant I (Actually an octant)
                        writtenPixels.Add(new XyCoordinate(rx, uy));
                    }
                    if (!clipLx && !clipUy)
                    {
                        pixels[lx + uh] = color; // Quadrant II
                        writtenPixels.Add(new XyCoordinate(lx, uy));
                    }
                    if (!clipLx && !clipLy)
                    {
                        pixels[lx + lh] = color; // Quadrant III
                        writtenPixels.Add(new XyCoordinate(lx, ly));
                    }
                    if (!clipRx && !clipLy)
                    {
                        pixels[rx + lh] = color; // Quadrant IV
                        writtenPixels.Add(new XyCoordinate(rx, ly));
                    }

                    x++;
                    xstopping += yrsqtwo;
                    err += xchg;
                    xchg += yrsqtwo;
                    if ((ychg + (err << 1)) > 0)
                    {
                        clipUy = false;
                        clipLy = false;

                        y--;
                        uy = yc + y;                  // Upper half
                        ly = yc - y;                  // Lower half

                        if (uy < 0)
                        {
                            clipUy = true;
                            uy = 0;
                        }
                        if (uy >= h)
                        {
                            clipUy = true;
                            uy = h - 1;
                        }
                        if (ly < 0)
                        {
                            clipLy = true;
                            ly = 0;
                        }
                        if (ly >= h)
                        {
                            clipLy = true;
                            ly = h - 1;
                        }

                        uh = uy * w;                  // Upper half
                        lh = ly * w;                  // Lower half
                        ystopping -= xrsqtwo;
                        err += ychg;
                        ychg += xrsqtwo;
                    }
                }
            }
        }
    }
}
