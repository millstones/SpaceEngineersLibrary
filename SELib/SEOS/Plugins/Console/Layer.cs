using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class Layer
    {
        public RectangleF Canvas;
        public Vector2 MinContentSize;
        
        public static Layer Vertical(float weight, Vector2 minContentSize) => new Layer
            {Canvas = new RectangleF(Vector2.Zero, new Vector2(weight, float.PositiveInfinity))};
        public static Layer OfViewport(Vector2 viewportSize) => new Layer
            {Canvas = new RectangleF(Vector2.Zero, viewportSize)};
        Layer() { }

        List<MySprite> Draw(Vector2 arrowPos, ConsoleStyle style,
            Func<string, float, Vector2> textMeasure, float textScale, out IInteractive interactive)
        {
            interactive = null;
            return new List<MySprite>();
        }
    }
}