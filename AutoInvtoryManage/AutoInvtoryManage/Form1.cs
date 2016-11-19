using com.github.KeyMove.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;

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
        string appPath = Application.StartupPath;
        BarCodePrint print;
        List<PictureBox> printPreViewList = new List<PictureBox>();
        Dictionary<int, string[]> printInfoMap = new Dictionary<int, string[]>();
        PictureBox SelectPrintPreBox;
        int RowIndex = -1;
        LoadExecl Loader = new LoadExecl();
        void InitESPUDP()
        {
            Client.NewUserCallBack((UserInfo info)=> {
                Invoke(new MethodInvoker(() =>
                {
                    A15ScanGunView.Items.Add(info);
                }));
                return info;
            });
            System.Windows.Forms.Timer T = new System.Windows.Forms.Timer();
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
                                if (A15RFScanGun.Checked)
                                {
                                    A1SelectIDText.Text = ds["物料代码"].ToString();
                                }
                            }
                            catch { info.SendString(lastRecvString = "showTime('    未知编号',200)"); return; }
                        }));
                        break;
                    case '2':break;
                    case '3':
                        if (A15RFScanGun.Checked) return;
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
                                    if (AOut(decodetime(values[1]), count))
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
                                    if(AIn(decodetime(values[1]), count))
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
            else if (str.StartsWith("["))
            {
                Invoke(new MethodInvoker(() =>
                {
                    try
                    {
                        string time = decodetime(str.Substring(0, str.IndexOf(']')));
                        DataRow dr = ExeclSheet.SheetData["库存信息$"].Rows.Find(time);
                        DataRow ds = ExeclSheet.SheetData["库存$"].Rows.Find(dr["物料代码"].ToString());
                        A1SelectIDText.Text = ds["物料代码"].ToString();
                    }
                    catch { }
                }));
                
            }
            else if (str.StartsWith("<"))
            {

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
        ComboBox A1FindType;
        DataGridView A1DataView;
        DataTable A1Table;
        private int printCol;
        private int printRow;
        private OpenFileDialog of=new OpenFileDialog();

        void InitUART()
        {
            COM = new SerialPort();
            COM.Parity = Parity.None;
            COM.DataBits = 8;
            COM.StopBits = StopBits.One;
            A15ComBps.SelectedIndex = 4;
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 500;
            t.Tick += (object sender, EventArgs e) => {
                string[] coms = SerialPort.GetPortNames();
                foreach (string s in coms)
                    if (!A15ComSelect.Items.Contains(s))
                    {
                        A15ComSelect.Items.Add(s);
                        if (A15ComSelect.SelectedIndex == -1)
                            A15ComSelect.SelectedIndex = 0;
                    }
            };
            t.Start();
            COM.DataReceived += (object sender, SerialDataReceivedEventArgs e)=> {
                if (!COM.IsOpen) return;
                string data = COM.ReadLine();
                if (data.StartsWith("["))
                {
                    data = data.Substring(1, data.IndexOf(']')-1);
                    data = decodetime(data);
                    Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            DataRow dr = ExeclSheet.SheetData["库存信息$"].Rows.Find(data);
                            DataRow ds = ExeclSheet.SheetData["库存$"].Rows.Find(dr["物料代码"].ToString());
                            A1SelectIDText.Text = ds["物料代码"].ToString();
                        }
                        catch { };
                    }));

                }
                else if (data.StartsWith("<"))
                {
                    data = data.Substring(1, data.IndexOf('>') - 1);
                    Invoke(new MethodInvoker(() =>
                    {
                        try
                        {
                            string[] info = data.Split(',');
                            data = decodetime(info[0]);
                            AOut(data, -int.Parse(info[1]));
                        }
                        catch { };
                    }));
                }
            };
        }
        

        private void Form1_Load(object sender, EventArgs e)
        {
            MainTabView.TabPages.Clear();
            MenuMap.Add(A1, new List<TabPage>(new TabPage[] { A11, A12, A13, A14, A15 }));
            MenuMap.Add(A2, new List<TabPage>(new TabPage[] { A21, /*A22*/ }));
            MenuMap.Add(A3, new List<TabPage>(new TabPage[] { A31 }));
            MenuMap.Add(A4, new List<TabPage>(new TabPage[] { A41, A42 }));
            MenuClick(A1, null);

            ButtonIndex.AddRange(new Button[]{A1,A2,A3,A4 });
            if (File.Exists(appPath + "\\DataBackup.xls"))
            {
                File.Delete(appPath + "\\Data.xls");
                File.Move(appPath + "\\DataBackup.xls", appPath + "\\Data.xls");
            }
            if (!File.Exists(appPath + "\\Data.xls"))
                CreateNewSheet(appPath + "\\Data.xls");
            ExeclSheet = new ExeclTools(appPath+"\\Data.xls");
            ExeclSheet.SheetData["库存$"].PrimaryKey = new DataColumn[] { ExeclSheet.SheetData["库存$"].Columns["物料代码"] };
            ExeclSheet.SheetData["库存信息$"].PrimaryKey = new DataColumn[] { ExeclSheet.SheetData["库存信息$"].Columns["日期"] };
            MainTabView_SelectedIndexChanged(null, null);
            //A11dataGridView.DataSource = ExeclSheet.SheetData["库存$"].DefaultView;
            A13TreeView.Nodes.Add("所有物料");
            InitESPUDP();
            InitUART();
            Code93.Font = Font;
            Printmap = new Bitmap(210, 297);
            Page = Graphics.FromImage(Printmap);
            Page.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            Page.PageUnit = GraphicsUnit.Millimeter;
            Page.PageScale = 1;            
            pictureBox1.Image = Printmap;
            PrintInit();
            A21FindType.SelectedIndex = A14FindType.SelectedIndex = A13FindType.SelectedIndex = A12FindType.SelectedIndex = A11FindType.SelectedIndex = 0;
        }

        void testprint(Graphics g)
        {

        }

        void buildImgBox()
        {
            int w = (int)(printDialog1.PrinterSettings.DefaultPageSettings.PaperSize.Width * 25.4 / 100 + 0.5);
            int h = (int)(printDialog1.PrinterSettings.DefaultPageSettings.PaperSize.Height * 25.4 / 100 + 0.5);
            DefBarCode db = new DefBarCode(w, h);
            printCol = db.col;
            printRow = db.row;
            Size s = new Size();
            s.Width = A21PrintView.Width / printCol;
            s.Height = A21PrintView.Height / printRow;
            print = db;
            foreach(PictureBox p in printPreViewList)
            {
                A21PrintView.Controls.Remove(p);
            }
            printPreViewList.Clear();
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
                        if (printPreViewList.Contains((PictureBox)sender))
                        {
                            SelectPrintPreBox = (PictureBox)sender;
                            //A21BarImage.Image = SelectPrintPreBox.Image;
                            A21PrintPos.Text = printPreViewList.IndexOf((PictureBox)sender).ToString();
                        }
                    };
                    p.DoubleClick += (object sender, EventArgs e) => {
                        if (printPreViewList.Contains((PictureBox)sender))
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
        }

        void PrintInit()
        {
            printDocument1.PrintPage += PrintDocument1_PrintPage;
            printPreviewDialog1.Document = printDocument1;
            printDialog1.Document = printDocument1;
            //A21PrintView.Document = printDocument1;
            //printDocument1.PrintController = new StandardPrintController();
            int w = (int)(printDialog1.PrinterSettings.DefaultPageSettings.PaperSize.Width * 25.4 / 100+0.5);
            int h = (int)(printDialog1.PrinterSettings.DefaultPageSettings.PaperSize.Height * 25.4 / 100+0.5);
            DefBarCode db = new DefBarCode(w,h);
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
                        if (printPreViewList.Contains((PictureBox)sender))
                        {
                            SelectPrintPreBox = (PictureBox)sender;
                            //A21BarImage.Image = SelectPrintPreBox.Image;
                            A21PrintPos.Text = printPreViewList.IndexOf((PictureBox)sender).ToString();
                        }
                    };
                    p.DoubleClick+= (object sender, EventArgs e) => {
                        if (printPreViewList.Contains((PictureBox)sender))
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
                            A1FindType = A11FindType;
                            break;
                        case 1:
                            A1DataView = A12dataGridView;
                            A1SelectCountText = A12CountText;
                            A1SelectIDText = A12IDText;
                            A1Table = ExeclSheet.SheetData["库存$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            A1FindType = A12FindType;
                            break;
                        case 2:
                            A1DataView = A13dataGridView;
                            //A1SelectCountText = A13CountText;
                            A1SelectIDText = A13IDText;
                            A1Table = ExeclSheet.SheetData["库存$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            A1FindType = A13FindType;
                            break;
                        case 3:
                            A1DataView = A14dataGridView;
                            //A1SelectCountText = A14CountText;
                            A1SelectIDText = A14IDText;
                            A1Table = ExeclSheet.SheetData["出入库$"];
                            A1DataView.DataSource = A1Table.DefaultView;
                            A1FindType = A14FindType;
                            break;
                    }
                    break;
                case 1:
                    switch (MainTabView.SelectedIndex)
                    {
                        case 0:
                            RowIndex = -1;
                            A1DataView = A21dataGridView;                            
                            A1SelectIDText = A21IDText;
                            DataTable dt = new DataTable();
                            A1FindType = A21FindType;
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
            DataRow[] drs = ExeclSheet.find("库存$", A1FindType.Text, A1SelectIDText.Text);
            //string name=A1Table.Columns[0].ToString();
            //drs = ExeclSheet.find("库存信息$", A1Table.Columns[0].ToString(), A1SelectIDText.Text);
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

        bool newInventoryIO(DataRow row, int count, string[] info,string timeinfo = null,bool write=true)
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
                if(write)
                    ExeclSheet.insert("出入库$",list.ToArray());
                else
                {
                    DataTable dt = ExeclSheet.SheetData["出入库$"];
                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < list.Count; i++)
                        dr[i] = list[i];
                    dt.Rows.Add(dr);
                }
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
                //sb.Append(dr.Table.Columns[i].ToString());
                sb.AppendLine();
            }
            sb.Append("操作:");
            sb.Append(count > 0 ? "入库" : "出库");
            return sb.ToString();
        }
        bool updateInveoryInfo(DataRow dr, int count,string time=null)
        {
            try
            {
                List<string> list = new List<string>();
                DataRow[] drs=null;
                string timeinfo = DateTime.Now.ToString("yyyy年MM月dd日HH:mm:ss");
                bool fifo = false;
                bool reqinfo = false;
                string[] datas = null;
                int index = 0;
                int storcount;
                if (time==null)
                {
                    storcount = int.Parse(dr["数量"].ToString());
                    if (count < 0)
                        if (storcount < -count)
                        {
                            return false;
                        }
                    fifo = dr["储存方式"].ToString() == "先进先出";
                    if (fifo)
                    {
                        if (count < 0)
                        {
                            drs = ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString());
                            foreach (var info in drs)
                                list.Add(info["日期"].ToString()+" (剩余:"+info["数量"].ToString()+")");
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
                            list.Add(info["储位"].ToString() + " (剩余:" + info["数量"].ToString() + ")");
                        if (MessageWindow["选择库存", buildinfo(dr, count), "储位", list.ToArray()].ShowDialog() != DialogResult.OK)
                            return false;
                    }
                    index = MessageWindow.SelectIndex;
                    datas = MessageWindow.Data;
                }
                else
                {
                    datas = new string[] { "", "", "" };
                    string code = ExeclSheet.find("库存信息$", "日期", time)[0]["物料代码"].ToString();
                    dr = ExeclSheet.find("库存$", "物料代码", code)[0];
                    drs = ExeclSheet.find("库存信息$", "物料代码", code);
                    bool equ = false;
                    for(int i=0;i<drs.Length;i++)
                        if(drs[i]["日期"].ToString()==time)
                        {
                            index = i;
                            equ = true;
                            break;
                        }
                    if (!equ) return false;
                    if (count > 0)
                        datas[0] = "扫码枪入库";
                    else
                        datas[0] = "扫码枪出库";
                }
                
                if (datas[0].Length == 0)
                    reqinfo = true;
                int len = int.Parse(dr["数量"].ToString());
                ExeclSheet.set("库存$", dr, "物料代码", "数量", (len + count).ToString());
                if (count < 0)
                {
                    int reqlen = -count;
                    DataRow ds = drs[index];
                    //var drs = ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString());
                    if (reqinfo)
                        datas[0] = "物料日期:" + ds["日期"].ToString();
                    int value = int.Parse(ds.ItemArray[1].ToString());
                    if (value > reqlen)
                    {
                        value -= reqlen;
                        ExeclSheet.set("库存信息$", ds, "日期", "数量", value.ToString());
                        newInventoryIO(dr, -reqlen, datas, timeinfo);
                        reqlen = 0;
                    }
                    else
                    {
                        if(fifo)
                            ExeclSheet.set("库存信息$", ds, "日期", "物料代码", "-");
                        else
                            ExeclSheet.set("库存信息$", ds, "日期", "数量", "0");
                        newInventoryIO(dr, -value, datas, timeinfo);
                        reqlen -= value;
                    }
                    if (reqlen != 0)
                        foreach (DataRow data in drs)
                        {
                            if (reqinfo)
                                datas[0] = "物料日期:" + data["日期"].ToString();
                            value = int.Parse(data.ItemArray[1].ToString());
                            if (value > reqlen)
                            {
                                value -= reqlen;
                                ExeclSheet.set("库存信息$", data, "日期", "数量", value.ToString());
                                newInventoryIO(dr, -reqlen, datas, timeinfo);
                                break;
                            }
                            else
                            {
                                if (fifo)
                                    ExeclSheet.set("库存信息$", data, "日期", "物料代码", "-");
                                else
                                    ExeclSheet.set("库存信息$", data, "日期", "数量", "0");
                                newInventoryIO(dr, -value, datas, timeinfo);
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
                            ExeclSheet.insert("库存信息$", new string[] { dr["物料代码"].ToString(), count.ToString(), timeinfo, dr["储位"].ToString() });
                    }
                    else
                    {
                        DataRow ds = drs[index];
                        ExeclSheet.set("库存信息$", ds, "日期", "数量", (int.Parse(ds["数量"].ToString()) + count).ToString());
                    }
                    newInventoryIO(dr, count, datas, timeinfo);
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

                int count = int.Parse(data[4]);
                DataRow[] drs = ExeclSheet.find("库存$", "物料代码", DataWindow.Data[0]);
                bool find = false;
                if (drs.Length > 0)
                {
                    int len = int.Parse(drs[0]["数量"].ToString());
                    foreach (DataRow dr in drs)
                    {
                        find = false;
                        if (data[5] != dr.ItemArray[5].ToString())
                        {
                            find = true;
                            break;
                        }
                        if (find)
                            break;
                    }
                    if (!find)
                    {
                        MessageBox.Show("不能添加已存在的物料代码");
                        return;
                    }
                    ExeclSheet.set("库存$", drs[0], "物料代码", "数量", (len + count).ToString());
                    DataRow[] rows = ExeclSheet.find("库存信息$", "物料代码", "-");
                    if (rows.Length != 0)
                        ExeclSheet.set("库存信息$", rows[0], "日期", new string[] { data[0], data[4], timeinfo, data[5] });
                    else
                        ExeclSheet.insert("库存信息$", new string[] { data[0], data[4], timeinfo, data[5] });
                    newInventoryIO(drs[0], int.Parse(data[4]), new string[] { "新储位", "", "" }, timeinfo);
                }
                else
                {
                    ExeclSheet.insert("库存$", data);
                    drs = ExeclSheet.find("库存$", "物料代码", DataWindow.Data[0]);
                    DataRow[] rows = ExeclSheet.find("库存信息$", "物料代码", "-");
                    if (rows.Length != 0)
                        ExeclSheet.set("库存信息$", rows[0], "日期", new string[] { data[0], data[4], timeinfo, data[5] });
                    else
                        ExeclSheet.insert("库存信息$", new string[] { data[0], data[4], timeinfo, data[5] });
                    newInventoryIO(drs[0], int.Parse(data[4]), new string[] { "新物料", "", "" }, timeinfo);
                    //ExeclSheet.insert("库存信息$", new string[] { drs[0]["物料代码"].ToString(), data[5], timeinfo, data[4] });
                }



                //DataRow dr = dt.NewRow();

            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (MessageBox.Show("退出程序?", "提示", MessageBoxButtons.OKCancel) == DialogResult.OK)
                base.OnClosing(e);
            else
                e.Cancel = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            ExeclSheet.Dispose();
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            base.OnClosed(e);
            System.Environment.Exit(0);
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
            int count = int.Parse(A1SelectCountText.Text);
            if (count == 0)
            {
                MessageBox.Show("数量不能为0!");
                return;
            }
            DataRow dr = ExeclSheet.SheetData["库存$"].Rows.Find(A1DataView.CurrentRow.Cells["物料代码"].Value.ToString());
            if(int.Parse(dr["数量"].ToString())< count)
            {
                MessageBox.Show("数量不足");
                return;
            }
            updateInveoryInfo(dr, -count);
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
                return updateInveoryInfo(null, req, id);
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
                return updateInveoryInfo(null, req, id);
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
            int count = int.Parse(A1SelectCountText.Text);
            if (count == 0)
            {
                MessageBox.Show("数量不能为0!");
                return;
            }
            DataRow dr = ExeclSheet.SheetData["库存$"].Rows.Find(A1DataView.CurrentRow.Cells["物料代码"].Value.ToString());
            updateInveoryInfo(dr, count);
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
            ASelectRow(null, null);
        }

        private void ASelectRow(object sender, EventArgs e)
        {
            if (A1DataView.CurrentRow != null)
            {
                A21AddPos.Enabled = true;
                if (RowIndex != A1DataView.CurrentRow.Index)
                {
                    A21ItemList.Items.Clear();
                    RowIndex = A1DataView.CurrentRow.Index;
                    DataRow[] rows = ExeclSheet.find("库存信息$", "物料代码", A1DataView.CurrentRow.Cells["物料代码"].Value.ToString());
                    //A21ItemList.Items.Clear();
                    foreach (DataRow dr in rows)
                        A21ItemList.Items.Add(dr["日期"].ToString());

                    A21ItemList.SelectedIndex = 0;
                    //A21BarImage.Image = Code93.BarCode(encodetime(A1DataView.CurrentRow.Cells["日期"].Value.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
                    //A21PrintView.Zoom = 1.5;
                    //A21PrintView.Show();
                }
            }
            else
            {
                RowIndex = -1;
                A21BarImage.Image = null;
                A21AddPos.Enabled = false;
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
                printInfoMap[index] = new string[] { encodetime(A21ItemList.SelectedItem.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString() };
                SelectPrintPreBox.Image = A21BarImage.Image;
                //foreach (DataGridViewRow dr in A1DataView.SelectedRows)
                //{
                //    printInfoMap[index] = new string[] { encodetime(A21ItemList.SelectedItem.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString() };
                //}
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

        private void A21ItemList_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            A21BarImage.Image = Code93.BarCode(encodetime(A21ItemList.SelectedItem.ToString()), A1DataView.CurrentRow.Cells["物料代码"].Value.ToString(), A1DataView.CurrentRow.Cells["物料名称"].Value.ToString() + A1DataView.CurrentRow.Cells["规格型号"].Value.ToString());
        }

        DateTime start = new DateTime(DateTime.Now.Ticks);
        private SerialPort COM;

        private void A13LoadExecl_Click(object sender, EventArgs e)
        {
            of.Filter = "表格|*.xls";
            if (of.ShowDialog() == DialogResult.OK)
            {
                string[] colname = new string[] { "物料代码", "物料名称", "规格型号", "单位", "数量", "储位", "储存方式" };
                A1DataView.DataSource = null;
                T2SConvert.ConvertType type=A13TSMode.Checked?T2SConvert.ConvertType.TRADITIONAL_CHINESE:T2SConvert.ConvertType.SIMPLIFIED_CHINESE;
                new Thread(() => {
                //Loader.CloseWindow = true;
                var reader = new ExeclTools(of.FileName);
                List<int> usecol = new List<int>();
                    TimeSpan sec = new TimeSpan(0, 0, 1);
                    //DateTime start = new DateTime(DateTime.Now.Ticks)-sec;
                    //DataTable Info = ExeclSheet.SheetData["出入库$"];
                    //DataTable InfoIO = ExeclSheet.SheetData["库存$"];
                    //DataTable InfoData = ExeclSheet.SheetData["库存信息$"];
                    List<string> names = new List<string>();
                    names.AddRange(reader.SheetData.Keys);
                    int index = 0;
                    foreach (var v in reader.SheetData.Values)
                    {
                        usecol.Clear();
                        bool use = false;
                        for (int i = 0; i < colname.Length; i++)
                        {
                            int pos;
                            if ((pos = v.Columns.IndexOf(colname[i])) != -1)
                            {
                                usecol.Add(pos);
                                use = true;
                            }
                            else
                                usecol.Add(-1);
                        }

                        if (use == false) continue;
                        int rowcount = v.Rows.Count * 10;
                        Invoke(new MethodInvoker(() =>
                        {
                            Loader.MaxValue = v.Rows.Count;
                        }));
                        DataRowCollection rows = ExeclSheet.SheetData["库存$"].Rows;
                        int count = 0;
                        Invoke(new MethodInvoker(() =>
                        {
                            Loader.ExtInfo = names[index++];
                            Loader.Value = count++;
                        }));
                        foreach (DataRow r in v.Rows)
                        {
                            string[] data = new string[] { "", "", "", "PCS", "0", "", "常规储存" };
                            for (int i = 0; i < colname.Length; i++)
                            {
                                if (usecol[i] != -1)
                                    data[i] = r.ItemArray[usecol[i]].ToString();
                            }
                            if (data[0].Length == 0) continue;
                            try
                            {
                                int.Parse(data[4]);
                            }
                            catch
                            {
                                continue;
                            }

                            data[1] = T2SConvert.ConvertString(data[1], type);
                            data[2] = T2SConvert.ConvertString(data[2], type);
                            DataRow dr = rows.Find(data[0]);
                            string timeinfo = start.ToString("yyyy年MM月dd日HH:mm:ss");
                            start -= (sec);
                            if (dr == null)
                            {
                                DataRow di = ExeclSheet.SheetData["库存$"].NewRow();
                                dr = di;
                                for (int i = 0; i < colname.Length; i++)
                                    di[colname[i]] = data[i];
                                ExeclSheet.SheetData["库存$"].Rows.Add(di);
                                di = ExeclSheet.SheetData["库存信息$"].NewRow();
                                di[0] = data[0];
                                di[1] = data[4];
                                di[2] = timeinfo;
                                di[3] = data[5];
                                ExeclSheet.SheetData["库存信息$"].Rows.Add(di);

                                //dr = ExeclSheet.insert("库存$", data);
                                //ExeclSheet.insert("库存信息$", new string[] { data[0], data[4], timeinfo, data[5] });
                            }
                            else
                            {
                                dr["数量"] = (int.Parse(dr["数量"].ToString()) + int.Parse(data[4])).ToString();
                                DataRow[] ds = ExeclSheet.find("库存信息$", "物料代码", data[0]);
                                bool find = false;
                                if (ds.Length != 0)
                                {
                                    foreach (DataRow d in ds)
                                    {
                                        if (d["储位"].ToString() == data[5])
                                        {
                                            find = true;
                                            d["数量"] = (int.Parse(d["数量"].ToString()) + int.Parse(data[4])).ToString();
                                            break;
                                        }
                                    }
                                }
                                if (!find)
                                {
                                    DataRow di = ExeclSheet.SheetData["库存信息$"].NewRow();
                                    di[0] = data[0];
                                    di[1] = data[4];
                                    di[2] = timeinfo;
                                    di[3] = data[5];
                                    ExeclSheet.SheetData["库存信息$"].Rows.Add(di);
                                }
                            }
                            //ExeclSheet.insert("库存信息$", new string[] { data[0], data[4], timeinfo, data[5] });
                            newInventoryIO(dr, int.Parse(data[4]), new string[] { "导入数据", "", "" }, timeinfo,false);
                            Invoke(new MethodInvoker(() =>
                            {
                                Loader.Value = count++;
                            }));
                        }

                    }
                    try
                    {
                        File.Move(appPath + "\\Data.xls", appPath + "\\DataBackup.xls");
                        CreateNewSheet(appPath + "\\Data.xls");
                        ExeclTools newSheet = new ExeclTools(appPath + "\\Data.xls");

                        Invoke(new MethodInvoker(() =>
                        {
                            Loader.Value = Loader.MaxValue;
                            Loader.Info = "保存中..";
                        }));
                        //ExeclSheet.update("库存$");
                        //ExeclSheet.update("库存信息$");
                        //ExeclSheet.update("出入库$");
                        ExeclSheet.Dispose();
                        newSheet.update("库存$", ExeclSheet.SheetData["库存$"]);
                        newSheet.update("库存信息$", ExeclSheet.SheetData["库存信息$"]);
                        newSheet.update("出入库$", ExeclSheet.SheetData["出入库$"]);
                        Invoke(new MethodInvoker(() =>
                        {
                            Loader.CloseWindow = true;
                        }));
                        //ExeclSheet = newSheet;
                        File.Delete(appPath + "\\DataBackup.xls");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.StackTrace+"\r\n"+ex.Message+"\r\n"+ex.ToString());
                    }
                }).Start();
                Loader.ShowDialog();
                MainTabView_SelectedIndexChanged(null, null);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x8000)
                notifyIcon1_MouseDoubleClick(null, null);
            base.WndProc(ref m);
        }
        private void A13EditDataButton_Click(object sender, EventArgs e)
        {
            DataRow dr = ExeclSheet.SheetData["库存$"].Rows.Find(A1DataView.CurrentRow.Cells["物料代码"].Value.ToString());
            string[] data = new string[dr.ItemArray.Length];
            for (int i = 0; i < data.Length; i++) data[i] = dr.ItemArray[i].ToString();
            DataRow[] drs = ExeclSheet.find("库存信息$", "物料代码", dr["物料代码"].ToString());
            string sournum = data[4];
            string sourpos = data[5];
            if (drs.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                StringBuilder sb2 = new StringBuilder();
                for(int i=0;i<drs.Length;i++)
                {
                    sb.Append(drs[i]["储位"].ToString());
                    sb2.Append(drs[i]["数量"].ToString());
                    if ((i + 1) < drs.Length)
                    {
                        sb.Append(',');
                        sb2.Append(',');
                    }
                }
                data[4] = sb2.ToString();
                data[5] = sb.ToString();
            }
            DataWindow.Data = data;
            if (DataWindow.ShowDialog() == DialogResult.OK)
            {
                string[] newdata = DataWindow.Data;
                string[] numvalue=null;
                string[] savepos=null;
                for(int j = 0; j < data.Length; j++)
                {
                    if (data[j] != newdata[j])
                    {
                        int num = 0;
                        if (data[4] != newdata[4]|| data[5] != newdata[5])
                        {
                            numvalue = newdata[4].Split(',');                            
                            savepos = newdata[5].Split(',');

                            if (numvalue.Length != drs.Length || savepos.Length != drs.Length)
                            {
                                MessageBox.Show("库存分类不匹配");
                                return;
                            }
                            for (int i = 0; i < numvalue.Length; i++)
                                num += int.Parse(numvalue[i]);
                            newdata[4] = num.ToString();
                            newdata[5] = savepos[0];
                        }
                        else
                        {
                            data[4] = newdata[4] = sournum;
                            data[5]= newdata[5] = sourpos;
                        }
                        for (int i = 0; i < drs.Length; i++)
                        {
                            if (data[0] != newdata[0])
                            {
                                ExeclSheet.set("库存信息$", drs[i], "日期", "物料代码", newdata[0]);
                            }
                            if (data[4] != newdata[4] || data[5] != newdata[5])
                            {
                                ExeclSheet.set("库存信息$", drs[i], "日期", "数量", numvalue[i]);
                                ExeclSheet.set("库存信息$", drs[i], "日期", "储位", savepos[i]);
                            }
                        }
                        ExeclSheet.set("库存$", dr, "物料代码", newdata);
                        newInventoryIO(dr, num, new string[] { "编辑物料","","" });
                        return;
                    }
                }
            }
        }

        private void PaintRowNum(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            using (SolidBrush b = new SolidBrush(this.ForeColor))
            {
                e.Graphics.DrawString(Convert.ToString(e.RowIndex + 1),
                e.InheritedRowStyle.Font, b, e.RowBounds.Location.X + 16, e.RowBounds.Location.Y + 4);
            }
        }

        private void 打开主界面ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExeclSheet.Dispose();
            notifyIcon1.Visible = false;
            notifyIcon1.Dispose();
            System.Environment.Exit(0);
        }

        public void showWindow()
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.ShowInTaskbar = false;
            }
        }

        void CreateNewSheet(string path)
        {
            FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
            byte[] data = AutoInvtoryManage.Properties.Resources.Base;
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }

        private void Save_Click(object sender, EventArgs e)
        {
            ExeclSheet.update("库存$");
            ExeclSheet.update("库存信息$");
            ExeclSheet.update("出入库$");
        }

        private void A21ConfigPrint_Click(object sender, EventArgs e)
        {
            printDialog1.ShowDialog();
            buildImgBox();
        }

        private void A15LinkCom_Click(object sender, EventArgs e)
        {
            if (COM.IsOpen)
            {
                A15LinkCom.Text = "连接";
                COM.Close();
                return;
            }
            COM.PortName = A15ComSelect.SelectedItem.ToString();
            COM.BaudRate = int.Parse(A15ComBps.SelectedItem.ToString());
            try { 
            COM.Open();
            }
            catch { MessageBox.Show("打开端口失败!");return; }
            A15LinkCom.Text = "断开";
        }
    }
}
