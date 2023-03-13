using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    interface IPageProvider
    {
        Page Page { get; }
    }

    interface IDrawer
    {
        ConsoleStyle Style { get; }
        Vector2 MeasureText(string txt, float scale);
    }

    public static class Layers
    {
        public static int BG = 0;
        public static int Content = 10;
        public static int Decals = 20;
        public static int FG = 30;
    }

    interface IInteractive
    {
        void OnSelect(ISurface surface);
        void OnEsc(ISurface surface);
        void OnInput(ISurface surface, Vector3 dir);
    }
    
    interface ISurface
    {
        void SwitchPage(string id);
        void SwitchPage(Page page);
        void SwitchPage<T>();
        void ShowMessageBox(string msg, RectangleF? viewport = null, int closeSec = int.MaxValue);
        void ShowMessageBox(FlexItem msg, RectangleF? viewport = null, int closeSec = int.MaxValue);
        void CloseMessageBox();
    }
}