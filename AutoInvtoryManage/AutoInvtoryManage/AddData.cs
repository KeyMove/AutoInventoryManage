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
    public partial class AddData : Form
    {
        public AddData()
        {
            InitializeComponent();
            T5.SelectedIndex = 0;
            T8.SelectedIndex = 0;
        }
        protected override void OnShown(EventArgs e)
        {
            T1.Text = ID.ToString("D8");
            base.OnShown(e);
        }
        public int ID
        {
            get; set;
        }

        public string[] Unit
        {
            get; set;
        }

        public string[] Data
        {
            get { return new string[] { /*T1.Text,*/T2.Text,T3.Text,T4.Text,T5.Text,T6.Text,T7.Text,T8.Text }; }
        }

        private void CSID_CheckedChanged(object sender, EventArgs e)
        {
            T1.ReadOnly = !CSID.Checked;
        }


        private void IntInput(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!(e.KeyChar >= '0' && e.KeyChar <= '9'))
                {
                      if (e.KeyChar != 8)
                        e.Handled = true;
                }
            }
            catch { }
        }
    }
}
