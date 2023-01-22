using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.New
{
    interface IInteractive
    {
        void OnClick(IConsole console);
        void OnHoverEnable(bool hover);
    }
    interface IConsolePage
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
    interface IDrawSurface
    {
        RectangleF Viewport { get; }
        Vector2 GridStep { get; }
        Vector2 ArrowPosition { get; }
        string FontId { get; }
        float FontScale { get; }
        void AddFrameSprites(List<MySprite> sprites);
        Vector2 MeasureText(string txt);
    }
}