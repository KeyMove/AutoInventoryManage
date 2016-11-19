using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AutoInvtoryManage
{
    class T2SConvert
    {
        public enum ConvertType:int
        {
            SIMPLIFIED_CHINESE= 0x02000000,
            TRADITIONAL_CHINESE = 0x04000000,
        }
        public static String ConvertString(String lines, ConvertType type)
        {
            Encoding gb2312 = Encoding.GetEncoding(936);
            byte[] src = gb2312.GetBytes(lines);
            byte[] dest = new byte[src.Length];
            LCMapString(0x0804, (int)type, src, -1, dest, src.Length);
            return gb2312.GetString(dest);
        }
        [DllImport("kernel32.dll", EntryPoint = "LCMapStringA")]
        public static extern int LCMapString(int Locale, int dwMapFlags, byte[] lpSrcStr, int cchSrc, byte[] lpDestStr, int cchDest);

        public const int LCMAP_SIMPLIFIED_CHINESE = 0x02000000;
        public const int LCMAP_TRADITIONAL_CHINESE = 0x04000000;
    }
}
