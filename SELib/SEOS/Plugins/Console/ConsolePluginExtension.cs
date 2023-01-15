using System;

namespace IngameScript
{
    static class ConsolePluginExtension
    {
        public static string ParseFont(this ConsoleFonts fonts)
        {
            return Enum.GetName(typeof(ConsoleFonts), fonts);
        }
        public static SEOS AddConsoleSite<T>(this SEOS seos, T site) where T : IUserContent
        {
            ConsolePlugin.RegisterPage(site);
            return seos;
        }
        public static SEOS UseCanvas(this SEOS seos, ConsoleManager consoleManager) 
        {
            ConsolePlugin.UseCanvas(consoleManager);
            return seos;
        }
    }
}