using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sandbox.ModAPI.Ingame;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class Display : IDrawer
    {
        public readonly RectangleF Viewport;

        public IInteractive Draw(FlexItem container, Vector2? arrowPos)
        {
            var content = container.Build(Viewport);
            var sprites = new List<MySprite>();

            Content selected = null;
            if (arrowPos.HasValue)
            {
                var c = content.Where(x => (x is IInteractive) && x.ContainsPoint(arrowPos.Value)).ToList();
                if (c.Any())
                    selected = c.MaxBy(x => x.Layer);
            }
            
            content.Sort((a, b) =>
            {
                if (a.Layer == b.Layer) return 0;
                if (a.Layer > b.Layer) return 1;

                return -1;
            });
            foreach (var c in content)
            {
                var isSelect = selected != null && selected == c;
                c.Draw(ref sprites, isSelect, this);
            }
            
            using (var frame = _surface.DrawFrame())
            {
                // frame.Clip((int) Viewport.Position.X, (int) Viewport.Position.Y, (int) Viewport.Width,
                //     (int) Viewport.Height);

                if (arrowPos.HasValue)
                {
                    sprites.Add(DrawArrow(arrowPos.Value));
                    var pos = MySprite.CreateText($"{arrowPos.Value.X:000}:{arrowPos.Value.Y:000}", 
                        ConsoleFonts.Monospace.ParseFont(), Color.Red, 0.25f, TextAlignment.LEFT);
                    pos.Position = arrowPos;
                    sprites.Add(pos);
                }
                
                frame.AddRange(sprites);
            }


            LastDrawnSpritesCount = sprites.Count;
            return selected as IInteractive;
        }
        public ConsoleStyle Style { get; }

        public int LastDrawnSpritesCount;
        
         public Vector2 MeasureText(string txt, float scale) =>
            _surface.MeasureStringInPixels(new StringBuilder(txt), Style.FontId, scale);
        

        readonly IMyTextSurface _surface;

        public Display(IMyTextSurface surface, ConsoleStyle style)
        {
            _surface = surface;
            Style = style;
            Viewport = SetupSurface();
        }
        RectangleF SetupSurface()
        {
            _surface.ContentType = ContentType.SCRIPT;
            //_panel.Script = "";
            _surface.ScriptBackgroundColor = Style.BGColor;// Style.Accent.Inverse();
            _surface.ScriptForegroundColor = Color.Black;

            var retVal = new RectangleF((_surface.TextureSize - _surface.SurfaceSize) / 2f, _surface.SurfaceSize);
            retVal.Position += ConsolePluginSetup.SCREEN_BORDER_PX;
            retVal.Size -= 2 * ConsolePluginSetup.SCREEN_BORDER_PX;

            return retVal;
        }

        MySprite DrawArrow(Vector2 arrowPosition)
        {
            return new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = "Triangle",
                Position = arrowPosition,
                Size = new Vector2(6, 9),
                Color = Style.BGColor.Inverse(), // Color.Lighten(Style.BGColorr, 0.8), //hsv.HSVtoColor(),
                RotationOrScale = -(float) Math.PI / 3f
            };
        }

        // MySprite DrawSelectRect(RectangleF viewport)
        // {
        //     return new MySprite
        //     {
        //         Type = SpriteType.TEXTURE,
        //         Data = "SquareSimple",
        //         Position = viewport.Center,
        //         Size = viewport.Size,
        //         Color = Color.Lighten(/*Style.BGColor*/Style.BGColor.Inverse(), 0.8f).Alpha(0.15f),
        //         Alignment = TextAlignment.CENTER
        //     };
        // }
    }
}