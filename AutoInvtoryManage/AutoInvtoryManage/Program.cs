using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace AutoInvtoryManage
{
    static class Program
    {

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool onlyone;
            using (Mutex mx = new Mutex(true, Application.ProductName, out onlyone))
            {
                if (!onlyone)
                {
                    Process[] ps = Process.GetProcessesByName(Application.ProductName);
                    if (ps.Length != 0)
                    {
                        //UdpClient c = new UdpClient(0);
                        //byte[] v = Encoding.Default.GetBytes("Show");
                        //IPEndPoint ep= new IPEndPoint(IPAddress.Parse("127.0.0.1"), 2333);
                        //c.Send(v, v.Length, ep);
                        return;
                    }
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
        }
    }
}
