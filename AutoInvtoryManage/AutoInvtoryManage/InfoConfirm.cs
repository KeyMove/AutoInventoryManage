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

        public InfoConfirm this[string info]
        {
            get { InfoText.Text = info; return this; }
        }

        public InfoConfirm this[string title,string info]
        {
            get { this.Text = title; InfoText.Text = info; return this; }
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
