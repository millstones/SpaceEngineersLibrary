

namespace IngameScript
{
    struct ParseLcdResult
    {
        public string SurfaceNameId;
        public int SurfaceInd;
        public string StartPageNameId;
    }

    struct ParseLcdControllerResult
    {
        public string ForLcdNameId;
    }
    static class ConsoleNameParser
    {
        public static ParseLcdResult ParseLcd(string customName)
        {
            var surfaceMark = ConsolePluginSetup.SURFACE_MARK;
            
            var lcdNameId = "";
            var surfaceInd = 0;
            var siteNameId = "";

            var nameReg = @"([a-zA-Z]*)";
            var surfaceReg = @"([0-9])";
            var siteNameReg = @"([a-zA-Z]*)";
            
            var match10 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}@{nameReg}-{siteNameReg}-{surfaceReg}");
            var match11 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}@{nameReg}-{surfaceReg}");
            var match20 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}@{nameReg}-{siteNameReg}");
            var match30 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}@{nameReg}");
            
            var match40 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}-{siteNameReg}-{surfaceReg}");
            var match50 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}-{surfaceReg}");
            var match60 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}-{siteNameReg}");


            if (match10.Success)
            {
                lcdNameId = match10.Groups[1].Value;
                siteNameId = match10.Groups[2].Value;
                surfaceInd = int.Parse(match10.Groups[3].Value);
            }
            else
            if (match11.Success)
            {
                lcdNameId = match11.Groups[1].Value;
                surfaceInd = int.Parse(match11.Groups[2].Value);
            }
            else
            if (match20.Success)
            {
                lcdNameId = match20.Groups[1].Value;
                siteNameId = match20.Groups[2].Value;
            }
            else
            if (match30.Success)
            {
                lcdNameId = match30.Groups[1].Value;
            }
            else
            if (match40.Success)
            {
                siteNameId = match40.Groups[1].Value;
                surfaceInd = int.Parse(match40.Groups[2].Value);
            }
            else
            if (match50.Success)
            {
                surfaceInd = int.Parse(match60.Groups[1].Value);
            }
            else
            if (match60.Success)
            {
                siteNameId = match50.Groups[1].Value;
            }

            return new ParseLcdResult
            {
                SurfaceNameId = lcdNameId,
                SurfaceInd = surfaceInd,
                StartPageNameId = siteNameId
            };
        }

        public static ParseLcdControllerResult ParseLcdController(string customName)
        {
            var surfaceMark = ConsolePluginSetup.SURFACE_CONTROLLER_MARK;
            
            var lcdNameId = "";
            
            var nameReg = @"([a-zA-Z]*)";
            var match30 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}@{nameReg}");
            if (match30.Success)
            {
                lcdNameId = match30.Groups[1].Value;
            }

            return new ParseLcdControllerResult
            {
                ForLcdNameId = lcdNameId
            };
        }
    }
}