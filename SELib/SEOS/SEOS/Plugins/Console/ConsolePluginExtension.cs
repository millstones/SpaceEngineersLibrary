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

        public static Color Inverse(this Color c) =>
            Color.FromNonPremultiplied(0xFF - c.R, 0xFF - c.G, 0xFF - c.B, c.A);
    }
}