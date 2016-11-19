using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoInvtoryManage
{
    public partial class LoadExecl : Form
    {
        string loadinfoext="";
        public LoadExecl()
        {
            InitializeComponent();
        }

        public int MaxValue
        {
            set { LoadBar.Maximum = value;LoadInfo.Text = loadinfoext+"导入中...0/" + value; }
            get { return LoadBar.Maximum; }
        }

        public int Value
        {
            set { LoadBar.Value = value; LoadInfo.Text = loadinfoext+"导入中..." + value + "/" + LoadBar.Maximum; }
            get { return LoadBar.Value; }
        }

        public string Info
        {
            set { LoadInfo.Text = value; }
            get { return LoadInfo.Text; }
        }

        public string ExtInfo
        {
            set { loadinfoext = value; }
            get { return loadinfoext; }
        }

        public bool CloseWindow
        {
            set { if (value) this.DialogResult = DialogResult.OK; }
        }


    }
}
