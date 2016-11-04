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
        List<PictureBox> printPreViewList = new List<PictureBox>();
        Dictionary<int, string[]> printInfoMap = new Dictionary<int, string[]>();
        PictureBox SelectPrintPreBox;
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

        string decodetime(string str)
        {
            return "20" + str.Insert(10, ":").Insert(8,":").Insert(6, "日").Insert(4, "月").Insert(2, "年");
        }

        string encodetime(string str)
        {
            return str.Remove(16, 1).Remove(13, 1).Remove(10, 1).Remove(7, 1).Remove(4, 1).Remove(0, 2);
        }

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
                            try
                            {
                                DataRow dr = ExeclSheet.SheetData["库存信息$"].Rows.Find(decodetime(values[1]));
                                DataRow ds = ExeclSheet.SheetData["库存$"].Rows.Find(dr["物料代码"].ToString());
                                info.SendString(("setInfo('" + ds["物料名称"].ToString() + ds["规格型号"].ToString() + "'," + dr["数量"].ToString() + ",'" + values[1] + "')").Replace("\r\n"," "));
                            }
                            catch { info.SendString(lastRecvString = "showTime('    未知编号',200)"); return; }
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
        private int printCol;
        private int printRow;

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
            ExeclSheet.SheetData["库存$"].PrimaryKey = new DataColumn[] { ExeclSheet.SheetData["库存$"].Columns["物料代码"] };
            ExeclSheet.SheetData["库存信息$"].PrimaryKey = new DataColumn[] { ExeclSheet.SheetData["库存信息$"].Columns["日期"] };
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
            //A21PrintView.Document = printDocument1;
            //printDocument1.PrintController = new StandardPrintController();
            DefBarCode db = new DefBarCode();
            printCol = db.col;
            printRow = db.row;
            Size s = new Size();
            s.Width = A21PrintView.Width / printCol;
            s.Height = A21PrintView.Height / printRow;
            print = db;
            A21PrintView.Resize += (object sender, EventArgs e) =>
            {
                s.Width = A21PrintView.Width / printCol;
                s.Height = A21PrintView.Height / printRow;
                for (int y = 0; y < printRow; y++)
                {
                    for (int x = 0; x < printCol; x++)
                    {
                        PictureBox p = printPreViewList[x+y*printCol];
                        p.Size = s;
                        p.Location = new Point(s.Width * x, s.Height * y);
                    }
                }
            };

            for (int y = 0; y < printRow; y++)
            {
                for (int x = 0; x < printCol; x++)
                {
                    PictureBox p = new PictureBox();
                    p.BorderStyle = BorderStyle.FixedSingle;
                    p.Size = s;
                    p.Location = new Point(s.Width * x, s.Height * y);
                    p.SizeMode = PictureBoxSizeMode.Zoom;
                    printPreViewList.Add(p);
                    p.Click += (object sender, EventArgs e) => {
                        if (printPreViewList.Contains(sender))
                        {
                            SelectPrintPreBox = (PictureBox)sender;
                            //A21BarImage.Image = SelectPrintPreBox.Image;
                            A21PrintPos.Text = printPreViewList.IndexOf((PictureBox)sender).ToString();
                        }
                    };
                    p.DoubleClick+= (object sender, EventArgs e) => {
                        if (printPreViewList.Contains(sender))
                        {
                            SelectPrintPreBox = (PictureBox)sender;
                            A21BarImage.Image = SelectPrintPreBox.Image;
                            A21PrintPos.Text = printPreViewList.IndexOf((PictureBox)sender).ToString();
                        }
                    };
                    A21PrintView.Controls.Add(p);
                }
            }
            SelectPrintPreBox = printPreViewList[0];
            A21BarPrintType.Items.Add(db);
            A21BarPrintType.SelectedIndex = 0;
        }

        private void PrintDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {

            int pos = int.Parse(A21PrintPos.Text);
            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            //testprint(e.Graphics);
            foreach (int index in printInfoMap.Keys)
            {
                string[] data = printInfoMap[index];
                print.Draw(e.Graphics, index, data[0],data[1],data[2]);
            }
            
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
                            DataTable dt = new DataTable();
                            foreach(DataRow dr in ExeclSheet.SheetData["库存$"].Rows)
                            {

                            }
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
            DataRow[] drs = ExeclSheet.find("库存$", A1Table.Columns[0].ToString(), A1SelectIDText.Text);
            //string name=A1Table.Columns[0].ToString();
            drs = ExeclSheet.find("先进先出$", A1Table.Columns[0].ToString(), A1SelectIDText.Text);
            foreach (DataRow dr in drs)
                dt.Rows.Add(dr.ItemArray);
            A1DataView.DataSource = dt.DefaultView;
        }

        string[] DataToRealTime(string[] data,string timeinfo)
        {
            List<string> list = new List<string>(data);
            list.Add(timeinfo);
            list.Add("入库");
            list.Add("新建库存");
            list.Add(" ");
            list.Add(" ");
            return list.ToArray();
        }


        void newInventoryItem(string[] info)
        {

        }

        bool newInventoryIO(DataRow row, int count, string[] info,string timeinfo = null)
        {
            try
            {
                if(timeinfo==null)
                    timeinfo = DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss");
                List<string> list = new List<string>();
                foreach (object o in row.ItemArray)
                    list.Add(o.ToString());
                list[row.Table.Columns.IndexOf("数量")] = (count < 0 ? -count : count).ToString();
                list.Add(timeinfo);
                list.Add((count > 0 ? "入库" : "出库"));
                foreach (object o in info)
                    list.Add(o.ToString());
                ExeclSheet.insert("出入库$",list.ToArray());
                return true;
            }
            catch { }
            return false;
        }


        string buildinfo(DataRow dr, int count)
        {
            StringBuilder sb = new StringBuilder();
            int index = dr.Table.Columns.IndexOf("数量");
            for (int i = 0; i < dr.ItemArray.Length; i++)
            {
                sb.Append(dr.Table.Columns[i].ToString());
                sb.Append(':');
                sb.Append(i == index ? (count < 0 ? -count : count).ToString() : dr.ItemArray[i].ToString());
                sb.Append(dr.Table.Columns[i].ToString());
            }
            sb.Append("操作:");
            sb.Append(count > 0 ? "入库" : "出库");
            return sb.ToString();
        }
        bool updateInveoryInfo(DataRow dr, int count)
        {
            try
            {
                List<string> list = new List<string>();
                DataRow[] drs=null;
                string timeinfo = DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss");
                bool fifo = dr["储存方式"].ToString() == "先进先出";
                if (fifo)
                {
                    if (count < 0)
                    {
                        drs = ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString());
                        foreach (var info in drs)
                            list.Add(info["日期"].ToString());
                        if (MessageWindow["选择库存", buildinfo(dr, count), "批次", list.ToArray()].ShowDialog() != DialogResult.OK)
                            return false;
                    }
                    else
                    {
                        if (MessageWindow["选择库存", buildinfo(dr, count)].ShowDialog() != DialogResult.OK)
                            return false;
                    }
                }
                else
                {
                    drs = ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString());
                    foreach (var info in drs)
                        list.Add(info["储位"].ToString());
                    if (MessageWindow["选择库存", buildinfo(dr, count), "储位", list.ToArray()].ShowDialog() != DialogResult.OK)
                        return false;
                }
                int len = int.Parse(dr["数量"].ToString());
                ExeclSheet.set("库存$", dr, "物料代码", "数量", (len + count).ToString());
                if (count < 0)
                {
                    int reqlen = -count;
                    //var drs = ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString());
                    foreach (DataRow data in ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString(), "日期"))
                    {
                        int value = int.Parse(data.ItemArray[1].ToString());
                        if (value > reqlen)
                        {
                            value -= reqlen;
                            ExeclSheet.set("库存信息$", data, "日期", "数量", value.ToString());
                            newInventoryIO(dr, -reqlen, MessageWindow.Data, timeinfo);
                            break;
                        }
                        else
                        {
                            ExeclSheet.set("库存信息$", data, "日期", "物料代码", "-");
                            newInventoryIO(dr, -value, MessageWindow.Data, timeinfo);
                            reqlen -= value;
                        }
                        if (reqlen == 0) break;
                    }
                }
                else
                {
                    if (fifo)
                    {
                        DataRow[] data = ExeclSheet.find("库存信息$", "物料代码", "-");
                        if (data.Length != 0)
                            ExeclSheet.set("库存信息$", data[0], "日期", new string[] { dr["物料代码"].ToString(), count.ToString(), timeinfo, dr["储位"].ToString() });
                        else
                            ExeclSheet.insert("库存信息$", new string[] { dr["物料代码"].ToString(), count.ToString(), timeinfo,dr["储位"].ToString() });
                    }
                    else
                    {
                        DataRow ds = drs[MessageWindow.SelectIndex];
                        ExeclSheet.set("库存信息$", ds, "日期","数量", (int.Parse(ds["数量"].ToString()) + count).ToString());
                    }
                    newInventoryIO(dr, count, MessageWindow.Data, timeinfo);
                }

                return true;
            }
            catch { }
            return false;
        }

        private void A13AddDataButton_Click(object sender, EventArgs e)
        {
            DataTable dt = ExeclSheet.SheetData["库存$"];
            DataWindow.ID = dt.Rows.Count;
            if (DataWindow.ShowDialog() == DialogResult.OK)
            {
                string[] data = DataWindow.Data;
                string timeinfo = DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss");
                if (data[6] == "先进先出")
                {
                    ExeclSheet.insert("先进先出$", new string[] { data[0], data[4], timeinfo });
                }
                else
                {
                    DataRow[] drs = ExeclSheet.find("库存$", "物料代码", DataWindow.Data[0]);
                    bool find = false;
                    foreach(DataRow dr in drs)
                    {
                        find = false;
                        for(int i = 0; i < data.Length; i++)
                        {
                            if (data[i] != dr.ItemArray[i].ToString())
                                break;
                            find = true;
                        }
                        if (find)
                            break;
                    }
                    if (!find)
                    {
                        MessageBox.Show("不能添加已存在的物料代码");
                        return;
                    }
                }
                List<string> v = new List<string>(DataWindow.Data);
                v.Add(timeinfo);
                ExeclSheet.insert("库存$",v.ToArray());
                ExeclSheet.insert("出入库$", DataToRealTime(DataWindow.Data, timeinfo));
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
            DataRow dr = ExeclSheet.SheetData["库存$"].Rows.Find(A1DataView.CurrentRow.Cells["物料代码"].Value.ToString());
            updateInveoryInfo(dr, -int.Parse(A1SelectCountText.Text));
            //int count = int.Parse(A1DataView.CurrentRow.Cells["数量"].Value.ToString());
            //int req = int.Parse(A1SelectCountText.Text);
            //if (count < req)
            //{
            //    MessageBox.Show("库存数量不足\r\n还缺少"+(req-count)+ A1DataView.CurrentRow.Cells["单位"].Value.ToString());
            //    return;
            //}
            //StringBuilder sb = new StringBuilder();
            //List<string> v = new List<string>();
            //int len = A1DataView.CurrentRow.Cells.Count;
            //for (int i = 0; i < len; i++)
            //    v.Add(A1DataView.CurrentRow.Cells[i].Value.ToString());
            //v[4] = (req).ToString();
            //for (int i = 0; i < len; i++)
            //{
            //    sb.Append(A1Table.Columns[i].ToString());
            //    sb.Append(':');
            //    sb.Append(v[i]);
            //    sb.Append("\r\n");
            //}

            //v[4] = (count - req).ToString();
            //if (sender != null)
            //{
            //    if (MessageWindow["出库管理",sb.ToString()].ShowDialog() != DialogResult.OK)
            //        return;
            //}
            //int reqlen = req;
            //if (v[6] == "先进先出")
            //{
            //    foreach (DataRow data in ExeclSheet.find("先进先出$", "物料代码", v[0],"日期"))
            //    {
            //        int value = int.Parse(data.ItemArray[1].ToString());
            //        if (value > reqlen)
            //        {
            //            value -= reqlen;
            //            ExeclSheet.set("先进先出$",data, "日期", "数量", value.ToString());
            //            break;
            //        }
            //        else
            //        {
            //            ExeclSheet.set("先进先出$", data, "日期", "物料代码", "-");
            //            reqlen -= value;
            //        }
            //        if (reqlen == 0) break;

            //    }
            //    //ExeclSheet.set("先进先出$", new string[] { v[0], v[4], DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss") });
            //}
            //ExeclSheet.set("库存$", ExeclSheet.SheetData["库存$"].Rows[A1DataView.CurrentRow.Index], "日期", "数量", v[4]);
            //v[4] = (req).ToString();
            //v[7] = (DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss"));
            //v.Add("出库");
            //v.AddRange(MessageWindow.Data);
            //ExeclSheet.insert("出入库$", v.ToArray());
        }

        bool AIn(string id,int req)
        {
            try
            {
                //DataTable dt = ExeclSheet.SheetData["库存$"];
                DataRow dr = ExeclSheet.SheetData["库存信息$"].Rows.Find(decodetime(id));
                DataRow ds = ExeclSheet.SheetData["库存$"].Rows.Find(dr["物料代码"].ToString());
               int count = int.Parse(dr["数量"].ToString());
                List<string> v = new List<string>();
                foreach (object s in dr.ItemArray)
                    v.Add(s.ToString());
                ExeclSheet.set("库存信息$", dr, "物料代码", "数量", (count + req).ToString());
                ExeclSheet.set("库存$", ds, "物料代码", "数量", (int.Parse(dr["数量"].ToString()) + req).ToString());
                v[ds.Table.Columns.IndexOf("数量")] = (req).ToString();
                v.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss"));
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
                DataRow dr = ExeclSheet.SheetData["库存信息$"].Rows.Find(decodetime(id));
                DataRow ds = ExeclSheet.SheetData["库存$"].Rows.Find(dr["物料代码"].ToString());
                int count = int.Parse(dr["数量"].ToString());
                if (count < req) return false;
                List<string> v = new List<string>();
                foreach (object s in dr.ItemArray)
                    v.Add(s.ToString());
                ExeclSheet.set("库存信息$", dr, "物料代码", "数量", (count - req).ToString());
                ExeclSheet.set("库存$", ds, "物料代码", "数量", (int.Parse(dr["数量"].ToString()) - req).ToString());
                v[ds.Table.Columns.IndexOf("数量")] = (req).ToString();
                v.Add(DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss"));
                v.Add("出库");
                v.Add("扫码枪出库");
                v.Add("");
                v.Add("");
                ExeclSheet.insert("出入库$", v.ToArray());
            }
            catch (Exception e) { return false; }
            return true;
        }

        private void A1In(object sender, EventArgs e)
        {
            if (A1DataView.CurrentRow == null)
            {
                MessageBox.Show("请选择一个物料");
                return;
            }
            DataRow dr = ExeclSheet.SheetData["库存$"].Rows.Find(A1DataView.CurrentRow.Cells["物料代码"].Value.ToString());
            updateInveoryInfo(dr, int.Parse(A1SelectCountText.Text));
            //int req = int.Parse(dr["数量"].ToString());
            //DataRow ds = ExeclSheet.SheetData["库存$"].Rows.Find(dr["物料代码"].ToString());
            //List<string> v = new List<string>();
            //foreach (object s in dr.ItemArray)
            //    v.Add(s.ToString());
            //v[ds.Table.Columns.IndexOf("数量")] = (req).ToString();
            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < dr.ItemArray.Length; i++)
            //{
            //    sb.Append(A1Table.Columns[i].ToString());
            //    sb.Append(':');
            //    sb.Append(v[i]);
            //    sb.Append("\r\n");
            //}
            //string timeinfo = DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss");
            //if (sender != null)
            //{
            //    if (MessageWindow["入库管理", sb.ToString()].ShowDialog() != DialogResult.OK)
            //        return;
            //    if (v[6] == "先进先出")
            //    {
            //        DataRow[] data = ExeclSheet.find("先进先出$", "物料代码", "-");
            //        if (data.Length!=0)
            //            ExeclSheet.set("先进先出$", data[0], "日期", new string[] { v[0], v[4], timeinfo });
            //        else
            //            ExeclSheet.insert("先进先出$", new string[] { v[0], v[4], timeinfo });
            //    }
            //}
            //ExeclSheet.set("库存$", A1DataView.CurrentRow.Index, "数量", (req+count).ToString());
            //v[ds.Table.Columns.IndexOf("数量")] = (req).ToString();
            //v.Add(timeinfo);
            //v.Add("入库");
            //v.AddRange(MessageWindow.Data);
            //ExeclSheet.insert("出入库$", v.ToArray());
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
            if(A1DataView.CurrentRow!=null)
                A21BarImage.Image = Code93.BarCode(encodetime(A1DataView.CurrentRow.Cells["日期"].Value.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString()+ A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
        }

        private void ASelectRow(object sender, EventArgs e)
        {
            if (RowIndex != A1DataView.CurrentRow.Index)
            {
                RowIndex = A1DataView.CurrentRow.Index;
                A21BarImage.Image = Code93.BarCode(encodetime(A1DataView.CurrentRow.Cells["日期"].Value.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
                //A21PrintView.Zoom = 1.5;
                //A21PrintView.Show();
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

        private void A21AddPos_Click(object sender, EventArgs e)
        {
            if (SelectPrintPreBox != null)
            {
                int index = printPreViewList.IndexOf(SelectPrintPreBox);
                printInfoMap[index] = new string[] { encodetime(A1DataView.CurrentRow.Cells["日期"].Value.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString() };
                SelectPrintPreBox.Image = A21BarImage.Image;
            }
        }

        private void A21ClearPos_Click(object sender, EventArgs e)
        {
            if (SelectPrintPreBox != null)
            {
                int index = printPreViewList.IndexOf(SelectPrintPreBox);
                printInfoMap.Remove(index);
                SelectPrintPreBox.Image = null;
            }
        }
    }
}
