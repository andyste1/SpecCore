﻿namespace SpecCoreLib
{
    public struct XyCoordinate
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public XyCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
