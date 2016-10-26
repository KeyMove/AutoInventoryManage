using com.github.KeyMove.Tools;
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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        AddData DataWindow = new AddData();
        ExeclTools ExeclSheet;
        Dictionary<Button, List<TabPage>> MenuMap = new Dictionary<Button, List<TabPage>>();
        List<Button> ButtonIndex = new List<Button>();
        Button SelectMenu = null;
        TextBox A1SelectCountText;
        TextBox A1SelectIDText;
        DataGridView A1DataView;
        DataTable A1Table;
        private void Form1_Load(object sender, EventArgs e)
        {
            MainTabView.TabPages.Clear();
            MenuMap.Add(A1, new List<TabPage>(new TabPage[] { A11, A12, A13, A14, A15 }));
            MenuMap.Add(A2, new List<TabPage>(new TabPage[] { A21, A22 }));
            MenuMap.Add(A3, new List<TabPage>(new TabPage[] { A31 }));
            MenuMap.Add(A4, new List<TabPage>(new TabPage[] { A41, A42 }));
            MenuClick(A1, null);

            ButtonIndex.AddRange(new Button[]{A1,A2,A3,A4 });

            ExeclSheet = new ExeclTools(System.Environment.CurrentDirectory+"\\test.xls");
            A11dataGridView.DataSource = ExeclSheet.SheetData["库存$"].DefaultView;
            A13TreeView.Nodes.Add("所有物料");
        }

        private void MenuClick(object sender, EventArgs e)
        {
            if (SelectMenu == sender) return;
            MainTabView.TabPages.Clear();
            SelectMenu = (Button)sender;
            MainTabView.TabPages.AddRange(MenuMap[SelectMenu].ToArray());
            MainTabView.SelectedIndex = 0;
        }

        private void MainTabView_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (ButtonIndex.IndexOf(SelectMenu))
            {
                case 0:
                    if (A1DataView != null)
                        A1DataView.DataSource = null;
                    switch (MainTabView.SelectedIndex)
                    {
                        case 0:
                            A1DataView = A11dataGridView;
                            A1SelectCountText = A11CountText;
                            A1SelectIDText = A11IDText;
                            A1Table = ExeclSheet.SheetData["库存$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            break;
                        case 1:
                            A1DataView = A12dataGridView;
                            A1SelectCountText = A12CountText;
                            A1SelectIDText = A12IDText;
                            A1Table = ExeclSheet.SheetData["库存$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            break;
                        case 2:
                            A1DataView = A13dataGridView;
                            //A1SelectCountText = A13CountText;
                            A1SelectIDText = A13IDText;
                            A1Table = ExeclSheet.SheetData["库存$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            break;
                        case 3:
                            A1DataView = A14dataGridView;
                            //A1SelectCountText = A14CountText;
                            A1SelectIDText = A14IDText;
                            A1Table = ExeclSheet.SheetData["出入库$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            break;
                    }
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }

        private void A1Find(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            string name=A1Table.Columns[0].ToString();
            dt.Rows.Add(A1Table.Select("[" + name + "]='" + A11IDText.Text+"'"));
            A1DataView.DataSource = dt.DefaultView;
        }

        string[] DataToRealTime(string[] data)
        {
            List<string> list = new List<string>(data);
            list.Add(DateTime.Now.ToShortDateString().ToString());
            list.Add(" ");
            list.Add(" ");
            return list.ToArray();
        }

        private void A13AddDataButton_Click(object sender, EventArgs e)
        {
            DataTable dt = ExeclSheet.SheetData["库存$"];
            DataWindow.ID = dt.Rows.Count;
            if (DataWindow.ShowDialog() == DialogResult.OK)
            {

                ExeclSheet.insert("库存$",DataWindow.Data);
                ExeclSheet.insert("出入库$", DataToRealTime(DataWindow.Data));
                //DataRow dr = dt.NewRow();

            }
        }

        protected override void OnClosed(EventArgs e)
        {
            ExeclSheet.Dispose();
            base.OnClosed(e);
        }

        private void IntInput(object sender, KeyPressEventArgs e)
        {
            WindowTools.IntInput(sender, e);
        }
    }
}
