using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace TranscriptReader
{
    public static class ColourDictionary
    {
        public static bool IsReadableForeground(Color foregroundColor, Color backgroundColor)
        {
            // Calculate the relative luminance of the foreground and background colors
            double foregroundLuminance = CalculateRelativeLuminance(foregroundColor);
            double backgroundLuminance = CalculateRelativeLuminance(backgroundColor);

            // Calculate the contrast ratio between the two colors
            double contrastRatio = (foregroundLuminance + 0.05) / (backgroundLuminance + 0.05);

            // Determine if the contrast ratio meets the WCAG 2.0 guidelines for readability
            const double minimumContrastRatio = 4.5;
            return contrastRatio >= minimumContrastRatio;
        }

        public static double CalculateRelativeLuminance(Color color)
        {
            // Convert color values to linear RGB
            double red = color.R / 255.0;
            double green = color.G / 255.0;
            double blue = color.B / 255.0;

            // Calculate relative luminance using the sRGB color space formula
            double redComponent = GetLinearRGBComponent(red);
            double greenComponent = GetLinearRGBComponent(green);
            double blueComponent = GetLinearRGBComponent(blue);

            return 0.2126 * redComponent + 0.7152 * greenComponent + 0.0722 * blueComponent;
        }

        public static double GetLinearRGBComponent(double component)
        {
            return component <= 0.03928 ? component / 12.92 : Math.Pow((component + 0.055) / 1.055, 2.4);
        }

        public static List<SolidColorBrush> LuminosityList = new();

        public static readonly List<SolidColorBrush> AllColours = new()
        {
            Brushes.AliceBlue,
            Brushes.AntiqueWhite,
            Brushes.Aqua,
            Brushes.Aquamarine,
            Brushes.Azure,
            Brushes.Beige,
            Brushes.Bisque,
            Brushes.Black,
            Brushes.BlanchedAlmond,
            Brushes.Blue,
            Brushes.BlueViolet,
            Brushes.Brown,
            Brushes.BurlyWood,
            Brushes.CadetBlue,
            Brushes.Chartreuse,
            Brushes.Chocolate,
            Brushes.Coral,
            Brushes.CornflowerBlue,
            Brushes.Cornsilk,
            Brushes.Crimson,
            Brushes.Cyan,
            Brushes.DarkBlue,
            Brushes.DarkCyan,
            Brushes.DarkGoldenrod,
            Brushes.DarkGray,
            Brushes.DarkGreen,
            Brushes.DarkKhaki,
            Brushes.DarkMagenta,
            Brushes.DarkOliveGreen,
            Brushes.DarkOrange,
            Brushes.DarkOrchid,
            Brushes.DarkRed,
            Brushes.DarkSalmon,
            Brushes.DarkSeaGreen,
            Brushes.DarkSlateBlue,
            Brushes.DarkSlateGray,
            Brushes.DarkTurquoise,
            Brushes.DarkViolet,
            Brushes.DeepPink,
            Brushes.DeepSkyBlue,
            Brushes.DimGray,
            Brushes.DodgerBlue,
            Brushes.Firebrick,
            Brushes.FloralWhite,
            Brushes.ForestGreen,
            Brushes.Fuchsia,
            Brushes.Gainsboro,
            Brushes.GhostWhite,
            Brushes.Gold,
            Brushes.Goldenrod,
            Brushes.Gray,
            Brushes.Green,
            Brushes.GreenYellow,
            Brushes.Honeydew,
            Brushes.HotPink,
            Brushes.IndianRed,
            Brushes.Indigo,
            Brushes.Ivory,
            Brushes.Khaki,
            Brushes.Lavender,
            Brushes.LavenderBlush,
            Brushes.LawnGreen,
            Brushes.LemonChiffon,
            Brushes.LightBlue,
            Brushes.LightCoral,
            Brushes.LightCyan,
            Brushes.LightGoldenrodYellow,
            Brushes.LightGray,
            Brushes.LightGreen,
            Brushes.LightPink,
            Brushes.LightSalmon,
            Brushes.LightSeaGreen,
            Brushes.LightSkyBlue,
            Brushes.LightSlateGray,
            Brushes.LightSteelBlue,
            Brushes.LightYellow,
            Brushes.Lime,
            Brushes.LimeGreen,
            Brushes.Linen,
            Brushes.Magenta,
            Brushes.Maroon,
            Brushes.MediumAquamarine,
            Brushes.MediumBlue,
            Brushes.MediumOrchid,
            Brushes.MediumPurple,
            Brushes.MediumSeaGreen,
            Brushes.MediumSlateBlue,
            Brushes.MediumSpringGreen,
            Brushes.MediumTurquoise,
            Brushes.MediumVioletRed,
            Brushes.MidnightBlue,
            Brushes.MintCream,
            Brushes.MistyRose,
            Brushes.Moccasin,
            Brushes.NavajoWhite,
            Brushes.Navy,
            Brushes.OldLace,
            Brushes.Olive,
            Brushes.OliveDrab,
            Brushes.Orange,
            Brushes.OrangeRed,
            Brushes.Orchid,
            Brushes.PaleGoldenrod,
            Brushes.PaleGreen,
            Brushes.PaleTurquoise,
            Brushes.PaleVioletRed,
            Brushes.PapayaWhip,
            Brushes.PeachPuff,
            Brushes.Peru,
            Brushes.Pink,
            Brushes.Plum,
            Brushes.PowderBlue,
            Brushes.Purple,
            Brushes.Red,
            Brushes.RosyBrown,
            Brushes.RoyalBlue,
            Brushes.SaddleBrown,
            Brushes.Salmon,
            Brushes.SandyBrown,
            Brushes.SeaGreen,
            Brushes.SeaShell,
            Brushes.Sienna,
            Brushes.Silver,
            Brushes.SkyBlue,
            Brushes.SlateBlue,
            Brushes.SlateGray,
            Brushes.Snow,
            Brushes.SpringGreen,
            Brushes.SteelBlue,
            Brushes.Tan,
            Brushes.Teal,
            Brushes.Thistle,
            Brushes.Tomato,
            Brushes.Transparent,
            Brushes.Turquoise,
            Brushes.Violet,
            Brushes.Wheat,
            Brushes.White,
            Brushes.WhiteSmoke,
            Brushes.Yellow,
            Brushes.YellowGreen
        };
    }
}
