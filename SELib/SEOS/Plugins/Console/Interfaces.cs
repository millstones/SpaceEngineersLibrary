using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    interface IInteractive
    {
        //bool IsMultipleClickSupport { get; }
        void OnSelect(IConsole console);
        void OnEsc(IConsole console);
        void OnInput(IConsole console, Vector3 dir);
    }
    interface IPageProvider
    {
        Page Page { get; }
    }
    interface IConsole
    {
        void SwitchPage(string id);
        void SwitchPage(Page page);
        void ShowMessageBox(string msg, RectangleF? viewport = null, int closeSec = int.MaxValue);
        void ShowMessageBox(MsgBoxItem msg, RectangleF? viewport = null, int closeSec = int.MaxValue);
        void CloseMessageBox();
    }
    interface ISurfaceDrawer
    {
        RectangleF Viewport { get; }
        Vector2 GridStep { get; }
        Vector2 ArrowPosition { get; }
        string FontId { get; }
        float FontSize { get; }
        ConsoleStyle Style { get; }
        Vector2 MeasureText(string txt, string fontId, float scale);
    }

    interface IText
    {
        float? FontSize { get; set; }
    }
}