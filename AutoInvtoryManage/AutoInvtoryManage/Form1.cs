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
            MainTabView_SelectedIndexChanged(null, null);
            //A11dataGridView.DataSource = ExeclSheet.SheetData["库存$"].DefaultView;
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
            DataTable dt = A1Table.Clone();
            string name=A1Table.Columns[0].ToString();
            foreach (DataRow dr in A1Table.Select("[" + name + "] like '" + A11IDText.Text + "'"))
                dt.Rows.Add(dr.ItemArray);
            A1DataView.DataSource = dt.DefaultView;
        }

        string[] DataToRealTime(string[] data)
        {
            List<string> list = new List<string>(data);
            list.Add(DateTime.Now.ToShortDateString().ToString());
            list.Add("入库");
            list.Add(" ");
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

        private void A1Enter(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) A1Find(null, null);
        }

        private void A1Out(object sender, EventArgs e)
        {
            if (A1DataView.CurrentRow == null)
            {
                MessageBox.Show("请选择一个物料");
                return;
            }
            int count = int.Parse(A1DataView.CurrentRow.Cells["数量"].Value.ToString());
            int req = int.Parse(A1SelectCountText.Text);
            if (count < req)
            {
                MessageBox.Show("库存数量不足\r\n还缺少"+(req-count)+ A1DataView.CurrentRow.Cells["单位"].Value.ToString());
                return;
            }
            List<string> v = new List<string>();
            int len = A1DataView.CurrentRow.Cells.Count;
            for (int i = 0; i < len; i++)
                v.Add(A11dataGridView.CurrentRow.Cells[i].Value.ToString());
            v[5] = (count - req).ToString();
            ExeclSheet.set("库存$", A1DataView.CurrentRow.Index, v.ToArray());
            v[5] = (req).ToString();
            v.Add("出库");
            v.Add(" ");
            v.Add(" ");
            v.Add(" ");
            ExeclSheet.insert("进出库$", v.ToArray());
        }
    }
}
