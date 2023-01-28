using System;
using System.Collections.Generic;
using VRage.Game.GUI.TextPanel;
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
            Border = true;
            Background = true;
            
            Add(new Text(() => Title)
                {
                    Alignment = Alignment.Center,
                    Margin = new Vector4(4),
                    Border = true
                },
                    CreateArea(Vector2.Zero, new Vector2(1, 0.1f)))
                ;
        }
    }

    class Page404 : Page
    {
        public Page404(string notFoundedPageName) : base(notFoundedPageName)
        {
            Title = "Error 404";
            Add(new Text($"Page '{notFoundedPageName}' NOT FOUND", 2, Color.Red), new RectangleF(Vector2.Zero, Vector2.One));
        }
    }

    class MessageBox : FreeCanvas
    {
        public MessageBox(string msg)
        {
            Border = true;
            Background = true;
            
            Add(new Text("'Title'"), CreateArea(Vector2.Zero, new Vector2(1, 0.15f)));
            Add(new Text(msg), CreateArea(new Vector2(0, 0.15f), Vector2.One));
        }
    }
}