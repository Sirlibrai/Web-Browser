using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebBrowser
{
    public class CustomTab : TabPage
    {
        public RichTextBox textbox;
        public BrowserHistory history;

        public CustomTab()
        {
            textbox = new RichTextBox();
            this.Controls.Add(textbox);
            textbox.Dock = DockStyle.Fill;
            history = new BrowserHistory();
            history.Urls = new List<string>();
        }
    }
}
