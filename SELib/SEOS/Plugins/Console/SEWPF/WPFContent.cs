using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript.SEWPF
{
    abstract class WPFItem
    {
        public Canvas Canvas { get; protected set; }
        protected ConsoleStyle Style;

        protected WPFItem(string def)
        {
            Canvas.ContentDefinition = new WPFContentDefinition(def);
        }
        
        // protected RectangleF SizeOfPadding(RectangleF rect)
        // {
        //     rect.Position += ConsolePluginSetup.PADDING_PX;
        //     rect.Size -= ConsolePluginSetup.PADDING_PX * 2;
        //
        //     return rect;
        // }
        protected MySprite GetSprite(string texture, RectangleF viewport, Color? color, float rotation = 0)
        {
            return new MySprite
            {
                Type = SpriteType.TEXTURE,
                Data = texture,
                Size = viewport.Size,
                Position = viewport.Center,
                Color = color,
                RotationOrScale = rotation,
                Alignment = TextAlignment.CENTER,
            };
        }

        protected MySprite GetText(string txt, RectangleF viewport, Color? color, float scale, string font)
        {
            return new MySprite
            {
                Type = SpriteType.TEXT,
                Data = txt,
                Size = viewport.Size,
                Position = viewport.Center,
                Color = color,
                RotationOrScale = scale,
                Alignment = TextAlignment.CENTER,
                FontId = font
            };
        }
        // protected MySprite GetText(string text, string fontId, float scale, RectangleF viewport, Color color,
        //     Func<string, float, Vector2> textMeasure)
        // {
        //     var textSize = textMeasure(text, scale);
        //     if (textSize.X > viewport.Width)
        //     {
        //         scale = viewport.Width / textSize.X;
        //     }
        //     if (textSize.Y > viewport.Height)
        //     {
        //         scale = viewport.Height / textSize.Y;
        //     }
        //     textSize = textMeasure(text, scale);
        //     /*
        //     if (Vertical)
        //     {
        //         return alignment == TextAlignment.LEFT
        //             ? viewport.Center - new Vector2(0, viewport.Height / 2f)
        //             : alignment == TextAlignment.RIGHT
        //                 ? viewport.Center + new Vector2(0, viewport.Height / 2f)
        //                 : viewport.Center;
        //     }
        //     */
        //     var pos = _alignment == TextAlignment.LEFT
        //         ? viewport.Center - new Vector2(viewport.Width / 2, textSize.Y / 2f)
        //         : _alignment == TextAlignment.RIGHT
        //             ? viewport.Center - new Vector2(-viewport.Width / 2, textSize.Y / 2f)
        //             : viewport.Center - new Vector2(0, textSize.Y / 2f);
        //
        //     return
        //         new MySprite
        //         {
        //             Type = SpriteType.TEXT,
        //             Data = text,
        //             Size = viewport.Size,
        //             Position = pos,
        //             Color = color,
        //             FontId = fontId,
        //             RotationOrScale = scale,
        //             Alignment = _alignment,
        //         };
        // }

        public void Resize(RectangleF viewport)
        {
            Canvas.Resize(viewport);
            OnResize(viewport);
        }

        public virtual void SetStyle(ConsoleStyle style)
        {
            Style = style;
        }
        protected virtual void OnResize(RectangleF viewport){}

        protected void DrawBG(ref List<MySprite> sprites)
        {
            sprites.Add(GetSprite("SquareSimple", Canvas.Viewport, Canvas.ContentDefinition.Color?? Style.FirstColor));
        }
        public abstract void Draw(ref List<MySprite> sprites);
    }
}