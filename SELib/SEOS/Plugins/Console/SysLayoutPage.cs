using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class SysLayoutPage : Page
    {
        protected SysLayoutPage() : base(ConsolePluginSetup.LOGO) { }
        public abstract void ShowMsgBox(string msg);
        public abstract void HideMsgBox();

        public abstract void SwitchUserContent(Page page);

        protected static RectangleF CreateArea(Vector2 leftUpPoint, Vector2 rightDownPoint)
        {
            rightDownPoint = Vector2.Clamp(rightDownPoint, Vector2.Zero, Vector2.One);
            leftUpPoint = Vector2.Clamp(leftUpPoint, Vector2.Zero, rightDownPoint);
            rightDownPoint = Vector2.Clamp(rightDownPoint, leftUpPoint, Vector2.One);

            return new RectangleF(leftUpPoint, rightDownPoint - leftUpPoint);
        }

        protected static RectangleF CalcViewport(RectangleF absViewport, RectangleF offset)
        {
            var position = absViewport.Position + offset.Position * absViewport.Size;
            var size = absViewport.Size * offset.Size;
                
            return new RectangleF(position, size);
        }
    }
    class DefaultSysLayoutPage : SysLayoutPage
    {
        Page _sysPanel, _userPanel;
        
        RectangleF sysArea, userArea, msgBoxArea;
        public DefaultSysLayoutPage()
        {
            sysArea = CreateArea(Vector2.Zero, new Vector2(1, 0.15f));
            userArea = CreateArea(new Vector2(0, 0.15f), Vector2.One);
            msgBoxArea = CreateArea(new Vector2(0.25f, 0.25f), new Vector2(0.75f, 0.75f));

            _sysPanel = new Page404("SUS PANEL");
            
            Add(_sysPanel);
            Add(_userPanel);
        }
        
        protected override void OnResize(RectangleF viewport)
        {
            Canvas.Resize(viewport);
            //base.OnResize(viewport);

            _sysPanel.Resize(CalcViewport(Canvas.Viewport, sysArea));
            _userPanel?.Resize(CalcViewport(Canvas.Viewport, userArea));
        }

        public override void ShowMsgBox(string msg)
        {
            
        }

        public override void HideMsgBox()
        {

        }

        public override void SwitchUserContent(Page page)
        {
            page.Resize(CalcViewport(Canvas.Viewport, userArea));
            _userPanel = page;
        }
    }
}