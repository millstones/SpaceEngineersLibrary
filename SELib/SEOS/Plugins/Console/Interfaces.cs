using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    interface IInteractive
    {
        //bool IsMultipleClickSupport { get; }
        void OnSelect(IConsole console, double power);
        void OnInput(IConsole console, Vector3 dir);
        void OnHoverEnable(bool hover);
    }
    interface IPageProvider
    {
        Page Page { get; }
    }
    interface IConsole
    {
        void SwitchPage(string id);
        void SwitchPage(Page page);
        void ShowMessageBox(string msg);
        void ShowMessageBox(MessageBox msg);
    }
    interface ISurfaceDrawer
    {
        RectangleF Viewport { get; }
        Vector2 GridStep { get; }
        Vector2 ArrowPosition { get; }
        string FontId { get; }
        float FontScale { get; }
        ConsoleStyle Style { get; }
        Vector2 MeasureText(string txt, string fontId, float scale);
    }
}