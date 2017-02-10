using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WebBrowser
{
    public partial class FavouritesForm : Form
    {
        private BrowserSettings settings;
        private MainForm main;

        public FavouritesForm(MainForm main)
        {
            InitializeComponent();
            this.main = main;
        }

        private void LoadFavourites()
        {
            // load from file
            listBox1.Items.Clear();
            settings = Helper.ReadFromJsonFile<BrowserSettings>("settings.txt");

            if (settings.Favourites != null)
            {
                foreach (var item in settings.Favourites)
                {
                    listBox1.Items.Add(item.Key + ":" + item.Value);
                }
            }
            else
                settings.Favourites = new Dictionary<string, string>();
        }

        private void SaveFavourites()
        {
            Helper.WriteToJsonFile<BrowserSettings>("settings.txt", settings, false);
        }

        private void FavouritesForm_Load(object sender, EventArgs e)
        {
            LoadFavourites();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            //Add new page            
            settings.Favourites.Add(txtName.Text, txtUrl.Text);
            SaveFavourites();
            LoadFavourites();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            //Save 
            if (listBox1.SelectedIndex == -1) return;

            settings.Favourites.Clear();

            listBox1.Items[listBox1.SelectedIndex] = txtName.Text + ":" + txtUrl.Text;

            foreach (var item in listBox1.Items)
            {
                settings.Favourites.Add(item.ToString().Split(':')[0], item.ToString().Split(':')[1]);
            }
            SaveFavourites();
            LoadFavourites();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            //Remove
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);

            settings.Favourites.Clear();
            foreach (var item in listBox1.Items)
            {
                settings.Favourites.Add(item.ToString().Split(':')[0], item.ToString().Split(':')[1]);
            }
            SaveFavourites();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //change 
            if (listBox1.SelectedIndex !=-1)
            {  txtName.Text = listBox1.SelectedItem.ToString().Split(':')[0];
               txtUrl.Text = listBox1.SelectedItem.ToString().Split(':')[1];
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if(listBox1.SelectedIndex!=-1)
            main.LoadPage(listBox1.Items[listBox1.SelectedIndex].ToString().Split(':')[1], null);
        }
    }
}
