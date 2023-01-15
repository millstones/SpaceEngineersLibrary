using System;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            


            var lcdResult = ConsoleNameParser.ParseLcd("Дисплей GUZUNOS LCD@Main-0");
            var lcdCtrlResult = ConsoleNameParser.ParseLcdController("Кресло пилота [GUZUNOS LCD CTRL@Main]");
            Console.WriteLine($"LcdName:{lcdResult.LcdNameId} - SurfaceId:{lcdResult.SurfaceInd} - SiteName:{lcdResult.SiteNameId}");
            Console.WriteLine($"LcdName:{lcdCtrlResult.LcdNameId}");
            
        }
    }
}
