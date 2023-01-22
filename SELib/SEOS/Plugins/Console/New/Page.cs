using System;
using VRageMath;

namespace IngameScript.New
{
    abstract class Page : Grid
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
            Add(new Text($"Page {notFoundedPageName} NOT FOUND"), Vector2.Zero, Vector2.One);
        }
    }

    class MessageBox : DockGrid
    {
        public MessageBox(string msg)
        {
            Add(new Text("MSG. 'title'"), Alignment.Up);
            Add(new Text(msg));
        }
    }
}