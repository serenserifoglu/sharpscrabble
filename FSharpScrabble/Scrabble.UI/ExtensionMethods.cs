using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Scrabble.UI
{
    /// <summary>
    /// Definitions for Extension methods used by the UI
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a hexidecimal string to a WPF Color object.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Color ToColor(this string hex)
        {
            ColorConverter converter = new ColorConverter();
            return (Color)converter.ConvertFrom(hex);
        }

        /// <summary>
        /// Converts a hexidecimal string to a WPF Brush object.
        /// </summary>
        /// <param name="hex"></param>
        /// <returns></returns>
        public static Brush ToBrush(this string hex)
        {
            BrushConverter bc = new BrushConverter();
            return (Brush)bc.ConvertFrom(hex);
        }
    }
}
