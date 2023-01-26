using System;
using VRage.Utils;
using VRageMath;

namespace IngameScript
{
    static class ConsolePluginSetup
    {
        public const string SURFACE_MARK = "GUZUNOS LCD";
        public const string SURFACE_CONTROLLER_MARK = "GUZUNOS LCD_CTRL";
        public const string DEBUG_LCD_MARK = "guzunos-debug";
        public const string LOGO = "'GUZUN OS'";
        public const int SCREEN_BORDER_PX = 3;
        public const int PADDING_PX = 1;
        public const int UPDATE_SURFACES_LIST_PERIOD_SEC = 10;
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
        public string FontId;
        
        public static ConsoleStyle Default => new ConsoleStyle
        {
            FirstColor = Color.White, 
            SecondColor = Color.Gray, 
            ThirdColor = Color.Black, 
            FontId = ConsoleFonts.White.ParseFont(),
        };

        public static ConsoleStyle MischieviousGreen => new ConsoleStyle
        {
            FirstColor = new Color(39, 39, 39),
            SecondColor = new Color(255, 101, 47),
            ThirdColor = new Color(20, 167, 108),
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };
        public static ConsoleStyle BlackWhiteRed => new ConsoleStyle
        {
            FirstColor = new Color(45, 48, 51),
            SecondColor = new Color(170, 75, 65),
            ThirdColor = new Color(212, 221, 225),
            FontId = ConsoleFonts.LoadingScreen.ParseFont(),
        };
    }
}