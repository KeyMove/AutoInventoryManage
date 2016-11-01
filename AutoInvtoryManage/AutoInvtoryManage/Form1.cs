using com.github.KeyMove.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
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

        Bitmap Printmap;
        Graphics Page;

        ESPUDP Client = new ESPUDP();
        List<UserInfo> ScanGunList = new List<UserInfo>();

        BarCodePrint print;

        int RowIndex = -1;
        void InitESPUDP()
        {
            Client.NewUserCallBack((UserInfo info)=> {
                Invoke(new MethodInvoker(() =>
                {
                    A15ScanGunView.Items.Add(info);
                }));
                return info;
            });
            Timer T = new Timer();
            T.Interval = 100;
            T.Tick += T_Tick;
            T.Start();
        }

        public class ScanGunInfo
        {
            public string id;
            public int check=0;
        }
        string lastRecvString;
        int lastRecvCheck=-1;
        void RecvCode(UserInfo info, byte[] data)
        {
            string str = Encoding.GetEncoding("GBK").GetString(data);
            if (str.StartsWith("ScanInfo:"))
            {
                string[] values = str.Substring(9).Split(',');
                switch (str[9])
                {
                    case '0':
                        break;
                    case '1':
                        
                        Invoke(new MethodInvoker(() =>
                        {
                            DataRow dr = ExeclSheet.SheetData["库存$"].Rows.Find(values[1]);
                            if (dr == null) return;
                            info.SendString("setInfo('" + dr["物料名称"].ToString() + dr["规格型号"].ToString() + "'," + dr["数量"].ToString() + ",'" + values[1] + "')");
                        }));
                        break;
                    case '2':break;
                    case '3':
                        int count = int.Parse(values[2]);
                        int check = int.Parse(values[3]);
                        if (lastRecvCheck == check)
                            info.SendString(lastRecvString);
                        else if (check != UseScanGunMap[info].check)
                        {
                            lastRecvCheck = check;
                            UseScanGunMap[info].check = check;
                            UseScanGunMap[info].id = values[1];
                            Invoke(new MethodInvoker(() =>
                            {
                                if (count < 0)
                                {
                                    if (AOut(values[1], -count))
                                    {
                                        info.SendString(lastRecvString = "showTime('    出库成功',200)");
                                    }
                                    else
                                    {
                                        info.SendString(lastRecvString = "showTime('    出库失败',200)");
                                    }
                                }
                                else
                                {
                                    if(AIn(values[1], count))
                                    {
                                        info.SendString(lastRecvString = "showTime('    入库成功',200)");
                                    }
                                    else
                                    {
                                        info.SendString(lastRecvString = "showTime('    入库失败',200)");
                                    }
                                }
                            }));
                        }
                        
                        break;
                }
            }
                    
        }

        Dictionary<UserInfo, ScanGunInfo> UseScanGunMap = new Dictionary<UserInfo, ScanGunInfo>();
        private void T_Tick(object sender, EventArgs e)
        {
            foreach(UserInfo info in A15UseScanView.Items)
            {
                info.SendString("getInfo()");
            }
        }

        AddData DataWindow = new AddData();
        InfoConfirm MessageWindow = new InfoConfirm();
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
            ExeclSheet.SheetData["库存$"].PrimaryKey = new DataColumn[] { ExeclSheet.SheetData["库存$"].Columns[0] };
            MainTabView_SelectedIndexChanged(null, null);
            //A11dataGridView.DataSource = ExeclSheet.SheetData["库存$"].DefaultView;
            A13TreeView.Nodes.Add("所有物料");
            InitESPUDP();
            Code93.Font = Font;
            Printmap = new Bitmap(210, 297);
            Page = Graphics.FromImage(Printmap);
            Page.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            Page.PageUnit = GraphicsUnit.Millimeter;
            Page.PageScale = 1;            
            pictureBox1.Image = Printmap;
            PrintInit();
        }

        void testprint(Graphics g)
        {

        }

        void PrintInit()
        {
            printDocument1.PrintPage += PrintDocument1_PrintPage;
            printPreviewDialog1.Document = printDocument1;
            printDialog1.Document = printDocument1;
            A21PrintView.Document = printDocument1;
            //printDocument1.PrintController = new StandardPrintController();
            print = new DefBarCode();
        }

        private void PrintDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            int pos = int.Parse(A21PrintPos.Text);
            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            //testprint(e.Graphics);
            print.Draw(e.Graphics, pos, A1DataView.CurrentRow.Cells["物料编号"].Value.ToString(), A1DataView.CurrentRow.Cells["物料编号"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
            
        }

        private void MenuClick(object sender, EventArgs e)
        {
            if (SelectMenu == sender) return;
            MainTabView.TabPages.Clear();
            SelectMenu = (Button)sender;
            MainTabView.TabPages.AddRange(MenuMap[SelectMenu].ToArray());
            MainTabView.SelectedIndex = 0;
            MainTabView_SelectedIndexChanged(null,null);
        }

        private void MainTabView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (A1DataView != null)
                A1DataView.DataSource = null;
            switch (ButtonIndex.IndexOf(SelectMenu))
            {
                case 0:
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
                    switch (MainTabView.SelectedIndex)
                    {
                        case 0:
                            A1DataView = A21dataGridView;                            
                            A1SelectIDText = A21IDText;
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
                    }
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
            GC.Collect();
        }

        private void A1Find(object sender, EventArgs e)
        {
            if(A1SelectIDText.Text.Length==0)
            {
                A1DataView.DataSource = A1Table.DefaultView;
                return;
            }
            DataTable dt = A1Table.Clone();
            string name=A1Table.Columns[0].ToString();
            foreach (DataRow dr in A1Table.Select("[" + name + "] like '" + A1SelectIDText.Text + "'"))
                dt.Rows.Add(dr.ItemArray);
            A1DataView.DataSource = dt.DefaultView;
        }

        string[] DataToRealTime(string[] data)
        {
            List<string> list = new List<string>(data);
            list.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss") );
            list.Add("入库");
            list.Add("新建库存");
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
            StringBuilder sb = new StringBuilder();
            List<string> v = new List<string>();
            int len = A1DataView.CurrentRow.Cells.Count;
            for (int i = 0; i < len; i++)
                v.Add(A1DataView.CurrentRow.Cells[i].Value.ToString());
            v[5] = (count - req).ToString();
            for (int i = 0; i < len; i++)
            {
                sb.Append(A1Table.Columns[i].ToString());
                sb.Append(':');
                sb.Append(v[i]);
                sb.Append("\r\n");
            }

            if (sender != null)
            {
                if (MessageWindow["出库管理",sb.ToString()].ShowDialog() != DialogResult.OK)
                    return;
            }
            ExeclSheet.set("库存$", A1DataView.CurrentRow.Index,"数量", v[5]);
            v[5] = (req).ToString();
            v.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss") );
            v.Add("出库");
            v.AddRange(MessageWindow.Data);
            ExeclSheet.insert("出入库$", v.ToArray());
        }

        bool AIn(string id,int req)
        {
            try
            {
                DataTable dt = ExeclSheet.SheetData["库存$"];
                DataRow dr = dt.Rows.Find(id);
                int index = dt.Rows.IndexOf(dr);
                int count = int.Parse(dr["数量"].ToString());
                List<string> v = new List<string>();
                foreach (object s in dr.ItemArray)
                    v.Add(s.ToString());
                ExeclSheet.set("库存$", index, "数量", (req + count).ToString());
                v[5] = (req).ToString();
                v.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss") );
                v.Add("入库");
                v.Add("扫码枪入库");
                v.Add("");
                v.Add("");
                ExeclSheet.insert("出入库$", v.ToArray());
            }
            catch { return false; }
            return true;
        }

        bool AOut(string id, int req)
        {
            try { 
            DataTable dt = ExeclSheet.SheetData["库存$"];
            DataRow dr = dt.Rows.Find(id);
            int index = dt.Rows.IndexOf(dr);
            int count = int.Parse(dr["数量"].ToString());
            if (count < req) return false;
            List<string> v = new List<string>();
            foreach (object s in dr.ItemArray)
                v.Add(s.ToString());
            ExeclSheet.set("库存$", index, "数量", (count - req).ToString());
            v[5] = (req).ToString();
            v.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss") );
            v.Add("出库");
            v.Add("扫码枪出库");
            v.Add("");
            v.Add("");
            ExeclSheet.insert("出入库$", v.ToArray());
            }
            catch(Exception e) { return false; }
            return true;
        }

        private void A1In(object sender, EventArgs e)
        {
            if (A1DataView.CurrentRow == null)
            {
                MessageBox.Show("请选择一个物料");
                return;
            }
            int count = int.Parse(A1DataView.CurrentRow.Cells["数量"].Value.ToString());
            int req = int.Parse(A1SelectCountText.Text);
            StringBuilder sb = new StringBuilder();
            List<string> v = new List<string>();
            int len = A1DataView.CurrentRow.Cells.Count;
            for (int i = 0; i < len; i++)
                v.Add(A1DataView.CurrentRow.Cells[i].Value.ToString());
            v[5] = (req).ToString();
            for (int i = 0; i < len; i++)
            {
                sb.Append(A1Table.Columns[i].ToString());
                sb.Append(':');
                sb.Append(v[i]);
                sb.Append("\r\n");
            }

            if (sender != null)
            {
                if (MessageWindow["入库管理", sb.ToString()].ShowDialog() != DialogResult.OK)
                    return;
            }
            ExeclSheet.set("库存$", A1DataView.CurrentRow.Index, "数量", (req+count).ToString());
            v[5] = (req).ToString();
            v.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss") );
            v.Add("入库");
            v.AddRange(MessageWindow.Data);
            ExeclSheet.insert("出入库$", v.ToArray());
        }

        private void A15LinkScanGun_Click(object sender, EventArgs e)
        {
            if (A15ScanGunView.SelectedItem == null) return;
            UserInfo info = (UserInfo)A15ScanGunView.SelectedItem;
            if (A15UseScanView.Items.Contains(info)) return;
            UseScanGunMap.Add(info, new ScanGunInfo());
            A15UseScanView.Items.Add((UserInfo)A15ScanGunView.SelectedItem);
            info.encode = Encoding.GetEncoding("GBK");
            info.RecvData(RecvCode);
        }

        private void A2Find(object sender, EventArgs e)
        {
            A1Find(sender, e);
            A21BarImage.Image = Code93.BarCode(A1DataView.CurrentRow.Cells["物料编号"].Value.ToString(), A1DataView.CurrentRow.Cells["物料编号"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString()+ A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
        }

        private void ASelectRow(object sender, EventArgs e)
        {
            if (RowIndex != A1DataView.CurrentRow.Index)
            {
                RowIndex = A1DataView.CurrentRow.Index;
                A21BarImage.Image = Code93.BarCode(A1DataView.CurrentRow.Cells["物料编号"].Value.ToString(), A1DataView.CurrentRow.Cells["物料编号"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
                A21PrintView.Zoom = 1.5;
                A21PrintView.Show();
            }
        }

        private void A21CopyBarImage_Click(object sender, EventArgs e)
        {
            Clipboard.SetData(DataFormats.Bitmap, A21BarImage.Image);
        }

        private void A21PrePrint_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.ShowDialog();
        }

        private void A21PrintBarCode_Click(object sender, EventArgs e)
        {
            printDocument1.Print();
        }
    }
}
