using System.Collections.Generic;
using VRageMath;

namespace IngameScript
{
    class Canvas
    {
        public RectangleF Viewport;

        public WPFContentDefinition ContentDefinition;

        public void Resize(RectangleF vpt)
        {
            var newVpt = vpt;
            if (ContentDefinition.ScaleSize.HasValue)
            {
                newVpt.Size *= ContentDefinition.ScaleSize.Value;
            }
            else 
            if (ContentDefinition.AbsSize.HasValue)
            {
                newVpt.Size = ContentDefinition.AbsSize.Value;
            }
            
            if (ContentDefinition.Margin.HasValue)
            {
                var margin = ContentDefinition.Margin.Value;
                newVpt.Size -= new Vector2(margin.X + margin.Y, margin.Z + margin.W);
                newVpt.Position += new Vector2(margin.X, margin.Z);
            }
            
            Viewport = newVpt;
        }
    }
}