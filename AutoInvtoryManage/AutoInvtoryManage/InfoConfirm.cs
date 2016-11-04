using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoInvtoryManage
{
    public partial class InfoConfirm : Form
    {

        protected override void OnShown(EventArgs e)
        {
            A1.Text = A2.Text = A3.Text = "";
            base.OnShown(e);
        }

        public string[] Data
        {
            get { return new string[] { A1.Text, A2.Text, A3.Text }; }
        }

        public string SelectItem
        {
            get { return B2.SelectedItem.ToString(); }
        }

        public int SelectIndex
        {
            get { return B2.SelectedIndex; }
        }

        public InfoConfirm this[string info]
        {
            get { InfoText.Text = info; B1.Visible = B2.Visible = false; return this; }
        }

        public InfoConfirm this[string title,string info]
        {
            get { this.Text = title; InfoText.Text = info; B1.Visible = B2.Visible = false; return this; }
        }

        public InfoConfirm this[string title, string info,string comboxinfo,string[] data]
        {
            get { this.Text = title; InfoText.Text = info; B1.Text = comboxinfo;B2.Items.Clear(); B2.Items.AddRange(data);B2.SelectedIndex = 0; return this; }
        }

        public InfoConfirm()
        {
            InitializeComponent();
        }


        private void InfoConfirm_Load(object sender, EventArgs e)
        {

        }
    }
}
