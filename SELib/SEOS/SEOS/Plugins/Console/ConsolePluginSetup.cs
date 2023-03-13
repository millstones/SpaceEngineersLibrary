using System;
using VRage.Utils;
using VRageMath;

namespace IngameScript
{
    static class ConsolePluginSetup
    {
        // example: 'name [BATTERY_GROUP_MARK@id]'
        public const string GROUP_MARK = "group";
        public const string SURFACE_MARK = "GUZUNOS LCD";
        public const string SURFACE_CONTROLLER_MARK = "GUZUNOS LCD_CTRL";
        public const string LOGO = "'GUZUN OS'";
        public const int SCREEN_BORDER_PX = 3;
        public const int PADDING_PX = 1;
        public const int SURFACES_LIST_UPDATE_PERIOD_SEC = 10;
        public const int POWER_PRODUCER_UPDATE_PERIOD_SEC = 10;
        public const float MOUSE_SENSITIVITY = 0.5f;
        public const bool ENABLE_AUTO_SWITCH_CONTROL_LCD = true;
        public const int MSG_SHOW_TIME_SEC = 3;


        public const string SURFACE_CONTROLLER_SWITCH_CTRL_MARK = "_switch_ctrl";
        public const string SURFACE_CONTROLLER_UP_CTRL_MARK = "_up_ctrl";
        public const string SURFACE_CONTROLLER_DOWN_CTRL_MARK = "_down_ctrl";
        public const string SURFACE_CONTROLLER_RIGHT_CTRL_MARK = "_right_ctrl";
        public const string SURFACE_CONTROLLER_LEFT_CTRL_MARK = "_left_ctrl";
        public const string SURFACE_CONTROLLER_SELECT_CTRL_MARK = "_select_ctrl";
        public const string SURFACE_CONTROLLER_DESELECT_CTRL_MARK = "deselect_ctrl";
    }

    enum Alignment
    {
        Center,
        CenterLeft, CenterUp, CenterDown, CenterRight,
        UpLeft, UpRight, DownLeft, DownRight
    }
    
    enum ConsoleFonts
    {
        White, LoadingScreen, Monospace
    }
    struct ConsoleStyle
    {
        public Color BGColor;
        public Color ContentColor;
        public Color Accent;
        public Color GoodAccent;
        public Color BadAccent;
        public string FontId;


        public static ConsoleStyle Default => new ConsoleStyle
        {
            BGColor = Color.Gray, 
            ContentColor = Color.Black, 
            Accent = Color.White, 
            GoodAccent = Color.Green,
            BadAccent = Color.Red,
            FontId = ConsoleFonts.White.ParseFont(),
        };

        public static ConsoleStyle MischieviousGreen => new ConsoleStyle
        {
            BGColor = new Color(39, 39, 39),
            ContentColor = new Color(255, 101, 47),
            Accent = new Color(20, 167, 108),
            GoodAccent = Color.Green,
            BadAccent = Color.Red,
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };
        public static ConsoleStyle MischieviousGreen2 => new ConsoleStyle
        {
            BGColor = new Color(76, 73, 71),
            ContentColor = new Color(206, 204, 206),
            Accent = new Color(34, 32, 34),
            GoodAccent = Color.Green,
            BadAccent = Color.Red,
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };

        public static ConsoleStyle BlackWhiteRed => new ConsoleStyle
        {
            BGColor = new Color(45, 48, 51),
            ContentColor = new Color(170, 75, 65),
            Accent = new Color(212, 221, 225),
            GoodAccent = Color.Green,
            BadAccent = Color.Red,
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };

        public static ConsoleStyle Complimentary => new ConsoleStyle
        {
            BGColor = "#0261ea".ColorFromHex(),
            ContentColor = "#000000".ColorFromHex(),
            Accent = "#e2ae15".ColorFromHex(),
            GoodAccent = "#552790".ColorFromHex(),
            BadAccent = "#b9110f".ColorFromHex(),
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };

        public static ConsoleStyle Lingua => new ConsoleStyle
        {
            BGColor = "#2c4653".ColorFromHex(),
            ContentColor = "#ffffff".ColorFromHex(),
            Accent = "#0e4d6c".ColorFromHex(),
            GoodAccent = "#e9ddc7".ColorFromHex(),
            BadAccent = "#f05833".ColorFromHex(),
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };
    }
}