using System;
using System.Collections.Generic;
using Microsoft.Win32;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class OverviewItem : InteractivePageItem
    {
        PageItem _simpleView;
        bool _showFullView;

        protected OverviewItem(PageItem simpleView)
        {
            _simpleView = simpleView;
            Select = console => _showFullView = true;
            Deselect = () => _showFullView = false;
        }

        protected override void DrawInternal(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive)
        {
            if (_showFullView)
                DrawFullView(drawer, ref viewport, ref sprites, ref interactive);
            else
                _simpleView.Draw(drawer, ref viewport, ref sprites, ref interactive);
        }

        protected abstract void DrawFullView(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites,
            ref IInteractive interactive);
    }

    class MenuOverview : OverviewItem
    {
        Menu _menu;
        public MenuOverview(string header, float? itemScale = null) 
            : base(new Text(header, itemScale) )
        {
            _menu = new Menu(itemScale);
        }

        public MenuOverview Add(string itemText, Action<IConsole> click)
        {
            _menu.Add(itemText, click);
            return this;
        }

        protected override void DrawFullView(ISurfaceDrawer drawer, ref RectangleF viewport, ref List<MySprite> sprites, ref IInteractive interactive)
        {
            _menu.Draw(drawer, ref viewport, ref sprites, ref interactive);
        }
    }
    
    
}