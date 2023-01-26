using System;
using VRageMath;

namespace IngameScript
{
    abstract class Page : FreeCanvas
    {
        public string Id;
        public string Title = ConsolePluginSetup.LOGO;

        protected Page(string id)
        {
            Id = id;
        }
    }

    class Page404 : Page
    {
        public Page404(string notFoundedPageName) : base(notFoundedPageName)
        {
            Title = "Error 404";
            Add(new Text($"Page '{notFoundedPageName}' NOT FOUND"), new RectangleF(Vector2.Zero, Vector2.One));
        }
    }

    class MessageBox : FreeCanvas
    {
        public MessageBox(string msg)
        {
            Add(new Text("MSG. 'title'"));
            Add(new Text(msg));
        }
    }
}