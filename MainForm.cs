using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebBrowser
{
    public partial class MainForm : Form
    {
        private BrowserSettings settings;
        private List<BrowserHistory> totalHistory;

        private CookieContainer cookie = new CookieContainer();

        public MainForm()
        {
            InitializeComponent();

            // Load settings (homepage &favourites)
            settings = Helper.ReadFromJsonFile<BrowserSettings>("settings.txt");

            if (settings == null)
            {
                settings = new BrowserSettings();
                settings.HomePage = "www.google.com";
                SaveSettings();
            }
            //Load browser history
            LoadHistory();
        }

        private void LoadHistory()
        {
            totalHistory = Helper.ReadFromJsonFile<List<BrowserHistory>>("history.txt");

            //In case of no file
            if (totalHistory == null)
                totalHistory = new List<BrowserHistory>();
            else
            {
                //Every tab is saved in totalHistory
                foreach (var item in totalHistory)
                {
                    CustomTab tab = new CustomTab();
                    tab.Text = item.Urls[item.currentIndex];
                    tab.textbox.Text = GetHTML(tab.Text, tab);
                    tab.history = item;


                    //Multi threading
                    //BrowserThread bthread = new BrowserThread(tab);
                    //Thread thread = new Thread(new ThreadStart(bthread.Start));
                    //thread.Start();
                    
                    //Adding Pages to control
                    tabControl.TabPages.Add(tab);
                    tabControl.SelectTab(tabControl.TabPages.Count - 1);
                }

                //Update current window history
                RefreshHistoryList(GetCurrentTab());                
            }

           
        }

        private void SaveHistory()
        {
            //If any page open then save to clean file
            if (tabControl.TabPages.Count > 0)
            {
                totalHistory = new List<BrowserHistory>();
                foreach (CustomTab item in tabControl.TabPages)
                {
                    totalHistory.Add(item.history);
                }

                Helper.WriteToJsonFile<List<BrowserHistory>>("history.txt", totalHistory, false);
            }
        }


        private CustomTab GetCurrentTab()
        {
            if (tabControl.TabPages.Count > 0)
                return (CustomTab)tabControl.TabPages[tabControl.SelectedIndex];
            else
                return null;
        }

        private void SaveSettings()
        {
            Helper.WriteToJsonFile<BrowserSettings>("settings.txt", settings, false);
        }
        

        private void btnSearch_Click(object sender, EventArgs e)
        {
            //Search by button
            LoadPage(urlTxt.Text, GetCurrentTab());
            //Update the current index in history to the newest page
            GetCurrentTab().history.currentIndex = GetCurrentTab().history.Urls.Count - 1;
        }

        public void LoadPage(string url, CustomTab tab)
        {
            //In case if this is a new tab
            if (tab == null)
            {
                tab = new CustomTab();

                tab.Text = url;
                tab.textbox.Text = GetHTML(url,tab);
                tabControl.TabPages.Add(tab);
                tabControl.SelectTab(tabControl.TabPages.Count - 1);
            }
            //Open in the current tab
            else
            {            
              tab.Text = url;
              tab.textbox.Text = GetHTML(url, tab);                                      
            }
        }


        private void AddToHistoryView(string url, CustomTab tab)
        {
            //Add url to tab history 
            if (tabControl.TabIndex != -1)
            {               
                tab.history.Urls.Add(url);
                
                //Refresh the history menu 
                RefreshHistoryList(tab);
                SaveHistory();
            }
        }

      
        public string GetHTML(string url, CustomTab tab)
        {
            //Add to menu history 
            AddToHistoryView(url, tab);

            string result = "";

            if (!url.Contains("http://"))
                url = "http://" + url;
            else if (!url.Contains("https://"))
                url = "https://" + url;

            try
            {                
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.CookieContainer = cookie;
                req.Method = "GET";
            
                req.KeepAlive = true;
                req.ContentLength = 0;                          
                req.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.48 Safari/537.36";
                req.ContentType = "application/x-www-form-urlencoded";
                req.Accept = "*/*";
               
                string s;

                HttpWebResponse webResp = (HttpWebResponse)req.GetResponse();
                Stream datastream = webResp.GetResponseStream();
                StreamReader reader = new StreamReader(datastream);
                s = reader.ReadToEnd();

                result += s;
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    if (response != null)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;

                        result += "Error code: " + (int)httpResponse.StatusCode + " " + httpResponse.StatusCode.ToString() + Environment.NewLine;
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            string text = reader.ReadToEnd();
                            result += text + Environment.NewLine;
                        }
                    }
                    else
                    {
                        result += "Error :"+ e.Message + Environment.NewLine;
                    }
                }
            }

            return result;
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btnHomePage_Click(object sender, EventArgs e)
        {
            LoadPage(settings.HomePage, null);
        }

        private void openFavouriToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                LoadPage(urlTxt.Text, GetCurrentTab());
                GetCurrentTab().history.currentIndex = GetCurrentTab().history.Urls.Count - 1;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (totalHistory == null)

            LoadPage(settings.HomePage, null);
        }

        private void homePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Homepage edit
            string value = settings.HomePage;
            if (Helper.InputBox("Homepage", "New homepage:", ref value) == DialogResult.OK)
            {
               settings.HomePage = value;
               SaveSettings();
            }
        }

        private void favouritesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FavouritesForm form = new FavouritesForm(this);
            form.Show();
        }

        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

   

        private void RefreshHistoryList(CustomTab tab)
        {
            if (tab != null)
            {
                //clear menu history
                historyToolStripMenuItem.DropDownItems.Clear();

                //add history from tab container
                foreach (var item in tab.history.Urls)
                {
                    ToolStripItem subItem = new ToolStripMenuItem(item);
                    subItem.Click += SubItem_Click;
                    historyToolStripMenuItem.DropDownItems.Add(subItem);
                }
            }

            
        }

        private void SubItem_Click(object sender, EventArgs e)
        {
            //Click on history menu
            ToolStripMenuItem tmp = (ToolStripMenuItem)sender;
            LoadPage(tmp.Text, GetCurrentTab());
        }

        private void newPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Create new tab
            LoadPage(settings.HomePage, null);
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Change history on page change
            RefreshHistoryList(GetCurrentTab());
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            //back button
            CustomTab tab = GetCurrentTab();
            if (tab.history.currentIndex - 1 >= 0)
            {
                tab.history.currentIndex--;
                LoadPage(tab.history.Urls[tab.history.currentIndex], tab);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //next button
            CustomTab tab = GetCurrentTab();
            if (tab.history.currentIndex + 1 <= tab.history.Urls.Count)
            {
                tab.history.currentIndex++;
                LoadPage(tab.history.Urls[tab.history.currentIndex], tab);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("This Program was designed by Ibrahim Tarfa");
        }

        private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void closePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //closes the current tab
            tabControl.TabPages.RemoveAt(tabControl.SelectedIndex);
        }

    
    }

    public class BrowserSettings
    {
        public string HomePage { get; set; }
        public Dictionary<string, string> Favourites { get; set; }
    }

    public class BrowserHistory
    {
        public List<string> Urls { get; set; }
        //Index of above list
        public int currentIndex { get; set; }
    }

    public class BrowserThread
    {
        public CustomTab tab { get; set; }

        public BrowserThread(CustomTab tab)
        {
            this.tab = tab;
        }

        public void Start()
        {

        }
    }
}
