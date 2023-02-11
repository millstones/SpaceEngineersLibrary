

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
        // 'name [SURFACE_MARK]' - текстовая панель с первой попавшийся страницей
        // 'name [SURFACE_MARK@id]' - текстовая панель с первой попавшийся страницей, id - id (имя) для управления
        // 'name [SURFACE_MARK-s]' - текстовая панель . s-имя страницы
        // 'name [SURFACE_MARK-n]' - текстовая панель многопанельного терм. блока с первой попавшийся страницей. n-номер текстовой панели
        // 'name [SURFACE_MARK-n-s]' - текстовая панель многопанельного терм. блока. n-номер текстовой панели, s-имя страницы
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
        
        
        /// <summary>
        /// Возвращает строку после разделителя или пустую строку.
        /// Должна начинаться с буквенного символа
        /// </summary>
        /// <param name="mark">маркер</param>
        /// <param name="customName">строка для поиска</param>
        /// <param name="delimetr">разделитель</param>
        /// <returns></returns>
        public static string FindSubstring(string mark, string customName, char delimetr='@')
        {
            var surfaceMark = mark;

            var nameReg = @"([a-zA-Z]*)";
            var match30 = System.Text.RegularExpressions.Regex.Match(customName, $@"{surfaceMark}{delimetr}{nameReg}");
            return match30.Success ? match30.Groups[1].Value : "";
        }
    }
}