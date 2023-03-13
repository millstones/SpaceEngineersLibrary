using System;
using VRageMath;

namespace IngameScript
{
    static class ConsolePluginExtension
    {
        public static string ParseFont(this ConsoleFonts fonts)
        {
            return Enum.GetName(typeof(ConsoleFonts), fonts);
        }
        public static SEOS AddConsoleSite<T>(this SEOS seos, T site) where T : IPageProvider
        {
            ConsolePlugin.RegisterPage(site);
            return seos;
        }

        // это не инверсия, а какаето херня
        public static Color Inverse(this Color c)
        => new Color((c.PackedValue/2 - uint.MaxValue)) {A = c.A};
        //=> new Color(0xFF - c.R, 0xFF - c.G, 0xFF - c.B, c.A);

        public static Color ColorFromHex(this string hex)
        {
            hex = hex.Replace("#", string.Empty);
            if (hex.Length == 6) hex = $"FF{hex}";
            var a = ConvertS2B(hex, 0, 2);
            var r = ConvertS2B(hex, 2, 2);
            var g = ConvertS2B(hex, 4, 2);
            var b = ConvertS2B(hex, 6, 2);
            
            return new Color(r, g, b, a);
        }
        
        static  byte ConvertS2B(string s, int a, int b) => (byte)Convert.ToUInt32(s.Substring(a, b), 16);
    }
}