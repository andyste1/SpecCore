namespace SpecCoreLib
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Glyph manager
    /// </summary>
    internal class GlyphManager
    {
        private Dictionary<char, byte[]> _glyphs;
        private Dictionary<char, byte[]> _graphics;

        /// <summary>
        /// Initializes a new instance of the <see cref="GlyphManager"/> class.
        /// </summary>
        internal GlyphManager()
        {
            CreateAsciiGlyphs();
            CreateStaticGraphicGlyphs();
        }

        /// <summary>
        /// Gets an ASCII glyph.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns></returns>
        internal byte[] GetAsciiGlyph(char c)
        {
            if (!_glyphs.ContainsKey(c))
            {
            }
            return _glyphs.TryGetValue(c, out var glyph) 
                ? glyph 
                : _glyphs['?'];
        }

        /// <summary>
        /// Gets a graphic glyph.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <returns></returns>
        internal byte[] GetGraphicGlyph(char c)
        {
            if (!_glyphs.ContainsKey(c))
            {
            }
            return _graphics.TryGetValue(c, out var glyph) 
                ? glyph 
                : _glyphs['?'];
        }

        /// <summary>
        /// Sets a graphic glyph.
        /// </summary>
        /// <param name="chr">The character.</param>
        /// <param name="data">The character data.</param>
        internal void SetGraphicGlyph(char chr, string[] data)
        {
            if (!char.IsLetter(chr) || !char.IsLower(chr))
            {
                throw new ArgumentException("You can only store a glyph against a lowercase letter");
            }

            if (data == null 
                || data.Length != 8 
                || data.Any(o => o.Length != 8) 
                || data.Any(o => o.Any(c => c != '0' && c != '1')))
            {
                throw new ArgumentException("The data must contain eight strings, each containing eight '0' or '1' characters");
            }

            // Convert the strings to bytes.
            var byteData = new byte[8];
            for (var i = 0; i < 8; i++)
            {
                var value = 0;
                var mult = 128;
                foreach (var c in data[i])
                {
                    if (c == '1')
                    {
                        value += mult;
                    }
                    mult = mult / 2;
                }
                byteData[i] = (byte)value;
            }

            // Store.
            _graphics[chr] = byteData;
        }

        /// <summary>
        /// Creates the ascii glyphs.
        /// </summary>
        private void CreateAsciiGlyphs()
        {
            _glyphs = new Dictionary<char, byte[]>();
            _glyphs[' '] = new byte[] 
            {
                    0x0,
                    0x0,
                    0x0,
                    0x0,
                    0x0,
                    0x0,
                    0x0,
                    0x0,
                };

            _glyphs['!'] = new byte[]
                {
                        0x00,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x00,
                        0x10,
                        0x00,
                };

            _glyphs['"'] = new byte[]
                {
                        0x00,
                        0x24,
                        0x24,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                };

            _glyphs['#'] = new byte[]
                {
                        0x00,
                        0x24,
                        0x7E,
                        0x24,
                        0x24,
                        0x7E,
                        0x24,
                        0x00,
                };

            _glyphs['$'] = new byte[]
                {
                        0x00,
                        0x08,
                        0x3E,
                        0x28,
                        0x3E,
                        0x0A,
                        0x3E,
                        0x08,
                };

            _glyphs['%'] = new byte[]
                {
                        0x00,
                        0x62,
                        0x64,
                        0x08,
                        0x10,
                        0x26,
                        0x46,
                        0x00,
                };

            _glyphs['&'] = new byte[]
                {
                        0x00,
                        0x10,
                        0x28,
                        0x10,
                        0x2A,
                        0x44,
                        0x3A,
                        0x00,
                };

            _glyphs['\''] = new byte[]
                {
                        0x00,
                        0x08,
                        0x10,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                };

            _glyphs['('] = new byte[]
                {
                        0x00,
                        0x04,
                        0x08,
                        0x08,
                        0x08,
                        0x08,
                        0x04,
                        0x00,
                };

            _glyphs[')'] = new byte[]
                {
                        0x00,
                        0x20,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x20,
                        0x00,
                };

            _glyphs['*'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x14,
                        0x08,
                        0x3E,
                        0x08,
                        0x14,
                        0x00,
                };

            _glyphs['+'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x08,
                        0x08,
                        0x3E,
                        0x08,
                        0x08,
                        0x00,
                };

            _glyphs[','] = new byte[]
                {
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x08,
                        0x08,
                        0x10,
                };

            _glyphs['-'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x3E,
                        0x00,
                        0x00,
                        0x00,
                };

            _glyphs['.'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x18,
                        0x18,
                        0x00,
                };

            _glyphs['/'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x02,
                        0x04,
                        0x08,
                        0x10,
                        0x20,
                        0x00,
                };

            _glyphs['0'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x46,
                        0x4A,
                        0x52,
                        0x62,
                        0x3C,
                        0x00,
                };

            _glyphs['1'] = new byte[]
                {
                        0x00,
                        0x18,
                        0x28,
                        0x08,
                        0x08,
                        0x08,
                        0x3E,
                        0x00,
                };

            _glyphs['2'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x02,
                        0x3C,
                        0x40,
                        0x7E,
                        0x00,
                };

            _glyphs['3'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x0C,
                        0x02,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['4'] = new byte[]
                {
                        0x00,
                        0x08,
                        0x18,
                        0x28,
                        0x48,
                        0x7E,
                        0x08,
                        0x00,
                };

            _glyphs['5'] = new byte[]
                {
                        0x00,
                        0x7E,
                        0x40,
                        0x7C,
                        0x02,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['6'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x40,
                        0x7C,
                        0x42,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['7'] = new byte[]
                {
                        0x00,
                        0x7E,
                        0x02,
                        0x04,
                        0x08,
                        0x10,
                        0x10,
                        0x00,
                };

            _glyphs['8'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x3C,
                        0x42,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['9'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x42,
                        0x3E,
                        0x02,
                        0x3C,
                        0x00,
                };

            _glyphs[':'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x00,
                        0x10,
                        0x00,
                        0x00,
                        0x10,
                        0x00,
                };

            _glyphs[';'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x10,
                        0x00,
                        0x00,
                        0x10,
                        0x10,
                        0x20,
                };

            _glyphs['<'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x04,
                        0x08,
                        0x10,
                        0x08,
                        0x04,
                        0x00,
                };

            _glyphs['='] = new byte[]
                {
                        0x00,
                        0x00,
                        0x00,
                        0x3E,
                        0x00,
                        0x3E,
                        0x00,
                        0x00,
                };

            _glyphs['>'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x10,
                        0x08,
                        0x04,
                        0x08,
                        0x10,
                        0x00,
                };

            _glyphs['?'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x04,
                        0x08,
                        0x00,
                        0x08,
                        0x00,
                };

            _glyphs['@'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x4A,
                        0x56,
                        0x5E,
                        0x40,
                        0x3C,
                        0x00,
                };

            _glyphs['A'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x42,
                        0x7E,
                        0x42,
                        0x42,
                        0x00,
                };

            _glyphs['B'] = new byte[]
                {
                        0x00,
                        0x7C,
                        0x42,
                        0x7C,
                        0x42,
                        0x42,
                        0x7C,
                        0x00,
                };

            _glyphs['C'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x40,
                        0x40,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['D'] = new byte[]
                {
                        0x00,
                        0x78,
                        0x44,
                        0x42,
                        0x42,
                        0x44,
                        0x78,
                        0x00,
                };

            _glyphs['E'] = new byte[]
                {
                        0x00,
                        0x7E,
                        0x40,
                        0x7C,
                        0x40,
                        0x40,
                        0x7E,
                        0x00,
                };

            _glyphs['F'] = new byte[]
                {
                        0x00,
                        0x7E,
                        0x40,
                        0x7C,
                        0x40,
                        0x40,
                        0x40,
                        0x00,
                };

            _glyphs['G'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x40,
                        0x4E,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['H'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x42,
                        0x7E,
                        0x42,
                        0x42,
                        0x42,
                        0x00,
                };

            _glyphs['I'] = new byte[]
                {
                        0x00,
                        0x3E,
                        0x08,
                        0x08,
                        0x08,
                        0x08,
                        0x3E,
                        0x00,
                };

            _glyphs['J'] = new byte[]
                {
                        0x00,
                        0x02,
                        0x02,
                        0x02,
                        0x42,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['K'] = new byte[]
                {
                        0x00,
                        0x44,
                        0x48,
                        0x70,
                        0x48,
                        0x44,
                        0x42,
                        0x00,
                };

            _glyphs['L'] = new byte[]
                {
                        0x00,
                        0x40,
                        0x40,
                        0x40,
                        0x40,
                        0x40,
                        0x7E,
                        0x00,
                };

            _glyphs['M'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x66,
                        0x5A,
                        0x42,
                        0x42,
                        0x42,
                        0x00,
                };

            _glyphs['N'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x62,
                        0x52,
                        0x4A,
                        0x46,
                        0x42,
                        0x00,
                };

            _glyphs['O'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x42,
                        0x42,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['P'] = new byte[]
                {
                        0x00,
                        0x7C,
                        0x42,
                        0x42,
                        0x7C,
                        0x40,
                        0x40,
                        0x00,
                };

            _glyphs['Q'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x42,
                        0x42,
                        0x52,
                        0x4A,
                        0x3C,
                        0x00,
                };

            _glyphs['R'] = new byte[]
                {
                        0x00,
                        0x7C,
                        0x42,
                        0x42,
                        0x7C,
                        0x44,
                        0x42,
                        0x00,
                };

            _glyphs['S'] = new byte[]
                {
                        0x00,
                        0x3C,
                        0x40,
                        0x3C,
                        0x02,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['T'] = new byte[]
                {
                        0x00,
                        0xFE,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x00,
                };

            _glyphs['U'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x42,
                        0x42,
                        0x42,
                        0x42,
                        0x3C,
                        0x00,
                };

            _glyphs['V'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x42,
                        0x42,
                        0x42,
                        0x24,
                        0x18,
                        0x00,
                };

            _glyphs['W'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x42,
                        0x42,
                        0x42,
                        0x5A,
                        0x24,
                        0x00,
                };

            _glyphs['X'] = new byte[]
                {
                        0x00,
                        0x42,
                        0x24,
                        0x18,
                        0x18,
                        0x24,
                        0x42,
                        0x00,
                };

            _glyphs['Y'] = new byte[]
                {
                        0x00,
                        0x82,
                        0x44,
                        0x28,
                        0x10,
                        0x10,
                        0x10,
                        0x00,
                };

            _glyphs['Z'] = new byte[]
                {
                        0x00,
                        0x7E,
                        0x04,
                        0x08,
                        0x10,
                        0x20,
                        0x7E,
                        0x00,
                };

            _glyphs['['] = new byte[]
                {
                        0x00,
                        0x0E,
                        0x08,
                        0x08,
                        0x08,
                        0x08,
                        0x0E,
                        0x00,
                };

            _glyphs['\\'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x40,
                        0x20,
                        0x10,
                        0x08,
                        0x04,
                        0x00,
                };

            _glyphs[']'] = new byte[]
                {
                        0x00,
                        0x70,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x70,
                        0x00,
                };

            _glyphs['^'] = new byte[]
                {
                        0x00,
                        0x10,
                        0x38,
                        0x54,
                        0x10,
                        0x10,
                        0x10,
                        0x00,
                };

            _glyphs['_'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0xFF,
                };

            _glyphs['`'] = new byte[]
                {
                        0x00,
                        0x1C,
                        0x22,
                        0x78,
                        0x20,
                        0x20,
                        0x7E,
                        0x00,
                };

            _glyphs['a'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x38,
                        0x04,
                        0x3C,
                        0x44,
                        0x3C,
                        0x00,
                };

            _glyphs['b'] = new byte[]
                {
                        0x00,
                        0x20,
                        0x20,
                        0x3C,
                        0x22,
                        0x22,
                        0x3C,
                        0x00,
                };

            _glyphs['c'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x1C,
                        0x20,
                        0x20,
                        0x20,
                        0x1C,
                        0x00,
                };

            _glyphs['d'] = new byte[]
                {
                        0x00,
                        0x04,
                        0x04,
                        0x3C,
                        0x44,
                        0x44,
                        0x3C,
                        0x00,
                };

            _glyphs['e'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x38,
                        0x44,
                        0x78,
                        0x40,
                        0x3C,
                        0x00,
                };

            _glyphs['f'] = new byte[]
                {
                        0x00,
                        0x0C,
                        0x10,
                        0x18,
                        0x10,
                        0x10,
                        0x10,
                        0x00,
                };

            _glyphs['g'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x3C,
                        0x44,
                        0x44,
                        0x3C,
                        0x04,
                        0x38,
                };

            _glyphs['h'] = new byte[]
                {
                        0x00,
                        0x40,
                        0x40,
                        0x78,
                        0x44,
                        0x44,
                        0x44,
                        0x00,
                };

            _glyphs['i'] = new byte[]
                {
                        0x00,
                        0x10,
                        0x00,
                        0x30,
                        0x10,
                        0x10,
                        0x38,
                        0x00,
                };

            _glyphs['j'] = new byte[]
                {
                        0x00,
                        0x04,
                        0x00,
                        0x04,
                        0x04,
                        0x04,
                        0x24,
                        0x18,
                };

            _glyphs['k'] = new byte[]
                {
                        0x00,
                        0x20,
                        0x28,
                        0x30,
                        0x30,
                        0x28,
                        0x24,
                        0x00,
                };

            _glyphs['l'] = new byte[]
                {
                        0x00,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x10,
                        0x0C,
                        0x00,
                };

            _glyphs['m'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x68,
                        0x54,
                        0x54,
                        0x54,
                        0x54,
                        0x00,
                };

            _glyphs['n'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x78,
                        0x44,
                        0x44,
                        0x44,
                        0x44,
                        0x00,
                };

            _glyphs['o'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x38,
                        0x44,
                        0x44,
                        0x44,
                        0x38,
                        0x00,
                };

            _glyphs['p'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x78,
                        0x44,
                        0x44,
                        0x78,
                        0x40,
                        0x40,
                };

            _glyphs['q'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x3C,
                        0x44,
                        0x44,
                        0x3C,
                        0x04,
                        0x06,
                };

            _glyphs['r'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x1C,
                        0x20,
                        0x20,
                        0x20,
                        0x20,
                        0x00,
                };

            _glyphs['s'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x38,
                        0x40,
                        0x38,
                        0x04,
                        0x78,
                        0x00,
                };

            _glyphs['t'] = new byte[]
                {
                        0x00,
                        0x10,
                        0x38,
                        0x10,
                        0x10,
                        0x10,
                        0x0C,
                        0x00,
                };

            _glyphs['u'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x44,
                        0x44,
                        0x44,
                        0x44,
                        0x38,
                        0x00,
                };

            _glyphs['v'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x44,
                        0x44,
                        0x28,
                        0x28,
                        0x10,
                        0x00,
                };

            _glyphs['w'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x44,
                        0x54,
                        0x54,
                        0x54,
                        0x28,
                        0x00,
                };

            _glyphs['x'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x44,
                        0x28,
                        0x10,
                        0x28,
                        0x44,
                        0x00,
                };

            _glyphs['y'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x44,
                        0x44,
                        0x44,
                        0x3C,
                        0x04,
                        0x38,
                };

            _glyphs['z'] = new byte[]
                {
                        0x00,
                        0x00,
                        0x7C,
                        0x08,
                        0x10,
                        0x20,
                        0x7C,
                        0x00,
                };

            _glyphs['{'] = new byte[]
                {
                        0x00,
                        0x0E,
                        0x08,
                        0x30,
                        0x08,
                        0x08,
                        0x0E,
                        0x00,
                };

            _glyphs['|'] = new byte[]
                {
                        0x00,
                        0x08,
                        0x08,
                        0x08,
                        0x08,
                        0x08,
                        0x08,
                        0x00,
                };

            _glyphs['}'] = new byte[]
                {
                        0x00,
                        0x70,
                        0x10,
                        0x0C,
                        0x10,
                        0x10,
                        0x70,
                        0x00,
                };

            _glyphs['~'] = new byte[]
                {
                        0x00,
                        0x14,
                        0x28,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                        0x00,
                };
        }

        /// <summary>
        /// Creates the static graphic glyphs.
        /// </summary>
        private void CreateStaticGraphicGlyphs()
        {
            _graphics = new Dictionary<char, byte[]>();

            // Top-left block
            _graphics['A'] = new byte[]
                           {
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                           };

            // Top-right block
            _graphics['B'] = new byte[]
                           {
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                           };

            // Bottom-left block
            _graphics['C'] = new byte[]
                           {
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                           };

            // Bottom-right block
            _graphics['D'] = new byte[]
                           {
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                           };

            // Top-half block
            _graphics['E'] = new byte[]
                           {
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                           };

            // Bottom-half block
            _graphics['F'] = new byte[]
                           {
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                           };

            // Left-half block
            _graphics['G'] = new byte[]
                           {
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                           };

            // Right-half block
            _graphics['H'] = new byte[]
                           {
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                           };

            // Right-half block
            _graphics['H'] = new byte[]
                           {
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                           };

            // Down step
            _graphics['I'] = new byte[]
                           {
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                           };

            // Up step
            _graphics['J'] = new byte[]
                           {
                               0x0F,
                               0x0F,
                               0x0F,
                               0x0F,
                               0xF0,
                               0xF0,
                               0xF0,
                               0xF0,
                           };

            // Solid
            _graphics['K'] = new byte[]
                           {
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                               0xFF,
                           };

            // Top-left block shaded
            _graphics['L'] = new byte[]
                           {
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                           };

            // Top-right block shaded
            _graphics['M'] = new byte[]
                           {
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                           };

            // Bottom-left block shaded
            _graphics['N'] = new byte[]
                           {
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                           };

            // Bottom-right block shaded
            _graphics['O'] = new byte[]
                           {
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                           };

            // Top-half block shaded
            _graphics['P'] = new byte[]
                           {
                               0xAA,
                               0x55,
                               0xAA,
                               0x55,
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                           };

            // Bottom-half block shaded
            _graphics['Q'] = new byte[]
                           {
                               0x0,
                               0x0,
                               0x0,
                               0x0,
                               0xAA,
                               0x55,
                               0xAA,
                               0x55,
                           };

            // Left-half block shaded
            _graphics['R'] = new byte[]
                           {
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                           };

            // Right-half block shaded
            _graphics['S'] = new byte[]
                           {
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                           };

            // Down step shaded
            _graphics['T'] = new byte[]
                           {
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                           };

            // Up step shaded
            _graphics['U'] = new byte[]
                           {
                               0x0A,
                               0x05,
                               0x0A,
                               0x05,
                               0xA0,
                               0x50,
                               0xA0,
                               0x50,
                           };

            // Checkerboard
            _graphics['V'] = new byte[]
                           {
                               0xAA,
                               0x55,
                               0xAA,
                               0x55,
                               0xAA,
                               0x55,
                               0xAA,
                               0x55,
                           };

            // Horizontal lines
            _graphics['W'] = new byte[]
                           {
                               0xFF,
                               0x00,
                               0xFF,
                               0x00,
                               0xFF,
                               0x00,
                               0xFF,
                               0x00,
                           };

            // Vertical lines
            _graphics['X'] = new byte[]
                           {
                               0xAA,
                               0xAA,
                               0xAA,
                               0xAA,
                               0xAA,
                               0xAA,
                               0xAA,
                               0xAA,
                           };
        }
    }
}
