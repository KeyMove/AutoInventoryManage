using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
            get { return new string[] { /*T1.Text,*/T2.Text, T3.Text, T4.Text, T5.Text, T6.Text, T7.Text, T8.Text }; }
            set { T2.Text = value[0]; T3.Text = value[1]; T4.Text = value[2]; T5.Text = value[3]; T6.Text = value[4]; T7.Text = value[5]; T8.Text = value[6]; }
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
