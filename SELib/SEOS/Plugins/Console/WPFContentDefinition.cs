using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    struct WPFContentDefinition
    {
        public Color? Color;
        public Vector4? Margin;    // left, right, up, down
        public Vector2? AbsSize, ScaleSize;

        public Alignment? Alignment;
        public bool? TextWrapping;

        public object Data;

        public WPFContentDefinition(string def)
        {
            this = Parse(def);
        }

        public static WPFContentDefinition Parse(string def)
        {
            var retVal = new RectangleF();



            return new WPFContentDefinition();
        }
    }
}