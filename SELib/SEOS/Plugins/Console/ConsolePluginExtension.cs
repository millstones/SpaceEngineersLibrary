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
        
        public static Point ToPoint(this Vector2I v) => new Point(v.X, v.Y);
        public static Point ToPoint(this Vector2 v) => new Point((int)v.X, (int)v.Y);
        public static Vector2I ToVector(this Point p) => new Vector2I(p.X, p.Y);
        public static Point Size(this Rectangle r) => new Point(r.Width, r.Height);
    }
}