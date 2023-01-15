using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    struct WPFContentDefinition
    {
#pragma warning disable 649
        public Color? Color;
        public Vector4? Margin;    // left, right, up, down
        public Vector2? AbsSize, ScaleSize;

        public Alignment? Alignment;
        public bool? TextWrapping;

        public object Data;
#pragma warning restore 649
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