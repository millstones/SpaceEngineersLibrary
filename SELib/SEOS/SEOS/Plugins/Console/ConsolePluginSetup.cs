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
        Center, Left, Right, Up, Down, CenterLeft, CenterUp, UpLeft, UpRight, DownLeft, DownRight
    }
    
    enum ConsoleFonts
    {
        White, LoadingScreen, Monospace
    }
    struct ConsoleStyle
    {
        public Color FirstColor;
        public Color SecondColor;
        public Color ThirdColor;
        public Color Akcent;
        public string FontId;


        public static ConsoleStyle Default => new ConsoleStyle
        {
            FirstColor = Color.Gray, 
            SecondColor = Color.Black, 
            ThirdColor = Color.White, 
            Akcent = Color.Green,
            FontId = ConsoleFonts.White.ParseFont(),
        };

        public static ConsoleStyle MischieviousGreen => new ConsoleStyle
        {
            FirstColor = new Color(39, 39, 39),
            SecondColor = new Color(255, 101, 47),
            ThirdColor = new Color(20, 167, 108),
            Akcent = Color.Green,
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };
        public static ConsoleStyle MischieviousGreen2 => new ConsoleStyle
        {
            FirstColor = new Color(76, 73, 71),
            SecondColor = new Color(206, 204, 206),
            ThirdColor = new Color(34, 32, 34),
            Akcent =  new Color(110, 173, 58),
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };

        public static ConsoleStyle BlackWhiteRed => new ConsoleStyle
        {
            FirstColor = new Color(45, 48, 51),
            SecondColor = new Color(170, 75, 65),
            ThirdColor = new Color(212, 221, 225),
            Akcent = Color.Green,
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };
    }
}