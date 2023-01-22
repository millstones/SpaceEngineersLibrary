using System;
using IngameScript.New;

namespace IngameScript
{
    static class ConsolePluginExtension
    {
        public static string ParseFont(this ConsoleFonts fonts)
        {
            return Enum.GetName(typeof(ConsoleFonts), fonts);
        }
        public static SEOS AddConsoleSite<T>(this SEOS seos, T site) where T : IConsolePage
        {
            ConsolePlugin.RegisterPage(site);
            return seos;
        }
    }
}