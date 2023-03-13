using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    abstract class SysPage : FlexItem// TabPanel
    {

        //Page _currentPage;
        Rectangle _currentPageViewport = new Rectangle(0,0,0, 0);
            
        FlexItem _currentMsgBoxItem;
        Rectangle _currentMsgBoxViewport = new Rectangle(1,1,-1, -1);
        
        protected SysPage(IEnumerable<Page> pages, Page startPage) //: base(pages.ToArray())
        {
            //Switch(startPage);
            
            var test = new TestPage();
            //Add(new ProgressBar(() => 0.25));
            Add(test);
            //Switch(startPage);
            //Switch(test);
        }

        public SysPage Switch(Page p)
        {
            return this;
        }
        public void ShowMessageBox(FlexItem msg, RectangleF? viewport = null, int closeSec = int.MaxValue)
        {
            CloseMessageBox();

            _currentMsgBoxItem = msg;
            // _currentMsgBoxViewport = viewport;
            // _drawer._lastInteractive = null;
                    
            Add(msg);
        }

        public void CloseMessageBox()
        {
            //RemoveAll(_currentMsgBoxItem);
            // _currentMsgBoxItem = null;
            // _currentMsgBoxViewport = null;
        }
    }

    class DefaultSysPage : SysPage
    {
        public DefaultSysPage(IEnumerable<Page> pages, Page startPage) : base(pages, startPage)
        {  }
    }

    class TestPage : Page
    {
        public TestPage()
        {
            // Direction = FlexDirection.Row;
            //Direction = FlexDirection.Column;
            // AlignItem = FlexAlignItem.Stretch;
            // AlignContent = FlexAlignContent.Start;
            //AlignContent = FlexAlignContent.End;
            // Justify = JustifyContent.SpaceAround;
            //Justify = JustifyContent.Center;
            // Size = FlexSize.FullViewport;
            // Order = 0;
            // Flex = 1;
            // Visible = true;

            Add(new FlexItemTestView(Color.Gray));
            Add(new FlexItemTestView(Color.WhiteSmoke));
        }
    }

    class FlexItemTestView : FlexItem
    {
        FlexItem _testItems;
        public FlexItemTestView(Color color) : base(new Rect {Color = color, Layer = Layers.BG, IsFill = true})
        {
            Direction = FlexDirection.Column;
            
            _testItems = new FlexItem();
            _testItems
                .Add(new FlexItem {Direction = FlexDirection.Row}
                    .Add(new Button(surface => { BtDirectionClick(); }, Color.YellowGreen))
                    .Add(new ProgressBar(() => DateTime.Now.Second / 60f), flex: 3)
                    .Add(new Rect {Color = Color.Red}, size: FlexSize.FromPixel(16 * 2, 9 * 2))
                )
                .Add(new FlexItem {Direction = FlexDirection.Row}
                    .Add(new Rect {Color = Color.Green})
                    .Add(new Rect {Color = Color.Yellow}, size: FlexSize.FromPixel(16 * 4, 9 * 4))
                    .Add(new Rect {Color = Color.OliveDrab}, size: FlexSize.FromPixel(16 * 2, 9 * 5))
                    .Add(new Rect {Color = Color.Aqua})
                )
                ;
            
            var btPanel = new FlexItem{Direction = FlexDirection.Row};
            var txtPanel = new FlexItem{Direction = FlexDirection.Row};
            btPanel
                .Add(new Button(s => BtDirectionClick(), "dir"))
                .Add(new Button(s => BtAltItemClick(), "alt itm"))
                .Add(new Button(s => BtAltContentClick(), "alt cnt"))
                .Add(new Button(s => BtJustClick(), "just"))
                .Add(new Button(s => Flex+=1, "flex +"))
                .Add(new Button(s => Flex-=1, "flex -"))
                ;
            txtPanel
                .Add(new Text(() => _testItems.Direction.ToString()))
                .Add(new Text(() => _testItems.AlignItem.ToString()))
                .Add(new Text(() => _testItems.AlignContent.ToString()))
                .Add(new Text(() => _testItems.Justify.ToString()))
                .Add(new Text(() => Flex.ToString()))
                .Add(new Text(() => Flex.ToString()))
                ;

            Add(_testItems);
            Add(btPanel);
            Add(txtPanel);
        }
        void BtDirectionClick()
        {
            _testItems.Direction = _testItems.Direction == FlexDirection.Column ? FlexDirection.Row : FlexDirection.Column;
        }
        void BtAltItemClick()
        {
            switch (_testItems.AlignItem)
            {
                case FlexAlignItem.Start:
                    _testItems.AlignItem = FlexAlignItem.End;
                    break;
                case FlexAlignItem.End:
                    _testItems.AlignItem = FlexAlignItem.Center;
                    break;
                case FlexAlignItem.Center:
                    _testItems.AlignItem = FlexAlignItem.Stretch;
                    break;
                case FlexAlignItem.Stretch:
                    _testItems.AlignItem = FlexAlignItem.Start;
                    break;
            }
        }
        void BtAltContentClick()
        {
            switch (_testItems.AlignContent)
            {
                case FlexAlignContent.Start:
                    _testItems.AlignContent = FlexAlignContent.End;
                    break;
                case FlexAlignContent.End:
                    _testItems.AlignContent = FlexAlignContent.Center;
                    break;
                case FlexAlignContent.Center:
                    _testItems.AlignContent = FlexAlignContent.Stretch;
                    break;
                case FlexAlignContent.Stretch:
                    _testItems.AlignContent = FlexAlignContent.SpaceBetween;
                    break;
                case FlexAlignContent.SpaceBetween:
                    _testItems.AlignContent = FlexAlignContent.SpaceAround;
                    break;
                case FlexAlignContent.SpaceAround:
                    _testItems.AlignContent = FlexAlignContent.Start;
                    break;
            }
        }
        void BtJustClick()
        {
            switch (_testItems.Justify)
            {
                case JustifyContent.Start:
                    _testItems.Justify = JustifyContent.End;
                    break;
                case JustifyContent.End:
                    _testItems.Justify = JustifyContent.Center;
                    break;
                case JustifyContent.Center:
                    _testItems.Justify = JustifyContent.SpaceBetween;
                    break;
                case JustifyContent.SpaceBetween:
                    _testItems.Justify = JustifyContent.SpaceAround;
                    break;
                case JustifyContent.SpaceAround:
                    _testItems.Justify = JustifyContent.SpaceEvery;
                    break;
                case JustifyContent.SpaceEvery:
                    _testItems.Justify = JustifyContent.Start;
                    break;
            }
        }
    }
}