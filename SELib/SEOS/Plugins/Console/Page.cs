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

    class MessageBoxItem : FreeCanvas, IInteractive
    {
        public MessageBoxItem(string msg)
        {
            Border = true;
            Background = true;

            Add(new Text("'Title'"), CreateArea(Vector2.Zero, new Vector2(1, 0.05f)));
            Add(new Text(msg), CreateArea(new Vector2(0, 0.05f), Vector2.One));
            Add(new Link("X", console => console.CloseMessageBox()) {Border = true},
                CreateArea(new Vector2(0.95f, 0), new Vector2(1, 0.05f)));
        }
        public MessageBoxItem(PageItem content)
        {
            Border = true;
            Background = true;

            Add(content);
            Add(new Link("X", console => console.CloseMessageBox()) {Border = true},
                CreateArea(new Vector2(0.95f, 0), new Vector2(1, 0.05f)));
        }
        
        public void OnSelect(IConsole console, double power)
        {
            if (power < -0.7) console.CloseMessageBox();
        }

        public void OnInput(IConsole console, Vector3 dir)
        {
        }

        public void OnHoverEnable(bool hover)
        {
        }
    }
    class MessageBoxItem<T> : MessageBoxItem
    {
        public Action<T> OnClose;

        public MessageBoxItem(string msg) : base(msg)
        {
            Enabled = false;
        }

        public MessageBoxItem(PageItem content) : base(content)
        {
            Enabled = false;
        }

        public void Show()
        {
            Enabled = true;
        }

        public void Close(T result)
        {
            Enabled = false;
            OnClose?.Invoke(result);
        }
    }
}