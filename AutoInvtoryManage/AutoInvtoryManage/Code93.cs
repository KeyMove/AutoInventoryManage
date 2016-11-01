using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.github.KeyMove.Tools
{
    public class Code93
    {
        static string[] FullASCIIEncode = new string[] { ")U", "(A", "(B", "(C", "(D", "(E", "(F", "(G", "(H", "(I", "(J", "(K", "(L", "(M", "(N", "(O", "(P", "(Q", "(R", "(S", "(T", "(U", "(V", "(W", "(X", "(Y", "(Z", ")A", ")B", ")C", ")D", ")E", " ", "#A", "#B", "#C", "$", "%", "#F", "#G", "#H", "#I", "#J", "+", "#L", "-", ".", "/", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "#Z", ")F", ")G", ")H", ")I", ")J", ")V", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", ")K", ")L", ")M", ")N", ")O", ")W", "@A", "@B", "@C", "@D", "@E", "@F", "@G", "@H", "@I", "@J", "@K", "@L", "@M", "@N", "@O", "@P", "@Q", "@R", "@S", "@T", "@U", "@V", "@W", "@X", "@Y", "@Z", ")P", ")Q", ")R", ")S" };
        static string[] Encode47 = new string[] { "100010100", "101001000", "101000100", "101000010", "100101000", "100100100", "100100010", "101010000", "100010010", "100001010", "110101000", "110100100", "110100010", "110010100", "110010010", "110001010", "101101000", "101100100", "101100010", "100110100", "100011010", "101011000", "101001100", "101000110", "100101100", "100010110", "110110100", "110110010", "110101100", "110100110", "110010110", "110011010", "101101100", "101100110", "100110110", "100111010", "100101110", "111010100", "111010010", "111001010", "101101110", "101110110", "110101110", "100100110", "111011010", "111010110", "100110010", "101011110" };
        static List<char> CharEncodeList = new List<char>(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '-', '.', ' ', '$', '/', '+', '%', '(', ')', '#', '@', '*', });
        static Font DefFont;
        public static Font Font
        {
            set { DefFont = value; }
            get { return DefFont; }
        }
        /// <summary>
        /// 把ASCII码转换为Code93支持的47个ASCII字符
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>返回转换结果</returns>
        static string ASCIItoCode47(string input)
        {
            StringBuilder sb = new StringBuilder();
            for(int i = 0; i < input.Length; i++)
            {
                if (input[i] >= 128)
                    throw new Exception("only support ASCII");
                sb.Append(FullASCIIEncode[input[i]]);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 添加Code93校验码
        /// </summary>
        /// <param name="input">转换过后的Code93字符</param>
        /// <returns>加入校验码的字符串</returns>
        static string AddCheckDigits(string input)
        {
            //checksum C
            int index = input.Length % 20;
            int sum = 0;
            if (index == 0) index = 20;
            for (int i = 0; i < input.Length; i++)
            {
                sum += CharEncodeList.IndexOf(input[i]) * index;
                if (--index == 0)
                    index = 20;
            }
            input += CharEncodeList[(sum % 47)];
            //checksum K
            sum = 0;
            index = input.Length % 15;
            if (index == 0) index = 15;
            for (int i = 0; i < input.Length; i++)
            {
                sum += CharEncodeList.IndexOf(input[i]) * index;
                if (--index == 0)
                    index = 15;
            }
            input += CharEncodeList[(sum % 47)];
            return input;
        }
        /// <summary>
        /// 把经过校验的Code93字符转换为条形码格式
        /// </summary>
        /// <param name="input">添加校验后的Code93字符串</param>
        /// <returns>Code93条形码字符串</returns>
        static string Code47toEncode(string input)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(Encode47[47]);
            for (int i = 0; i < input.Length; i++)
                sb.Append(Encode47[CharEncodeList.IndexOf(input[i])]);
            sb.Append(Encode47[47]);
            sb.Append(1);
            return sb.ToString();
        }

        static Bitmap ASCIItoBitmap(string str,int w,int h,string message,string title)
        {
            if (message != null && DefFont != null)
            {
                h+=(int)((DefFont.Size * 32 / 24)+8)*(title!=null?2:1);
            }
            Bitmap map = new Bitmap(w, h);
            Graphics Draw = Graphics.FromImage(map);
            Draw.Clear(Color.White);
            if (w / str.Length > 1)
            {
                w = w / str.Length;
                h = (map.Width - (w * str.Length)) / 2;
                for(int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '1')
                        Draw.FillRectangle(Brushes.Black, h + i * w, 4, w, map.Height);
                }
            }
            else
            {
                for(int i = 0; i < str.Length; i++)
                {
                    if (str[i] == '1')
                        Draw.DrawLine(Pens.Black, i, 4, i, map.Height);
                }
            }
            if (message != null&&DefFont!=null)
            {
                int cw = (int)(DefFont.Size*32/48);
                int charwith = 0;
                for (int i = 0; i < message.Length; i++)
                {
                    if (message[i] < 128) charwith += cw;
                    else charwith += cw * 2;
                }
                Draw.FillRectangle(Brushes.White, 0, map.Height - 8 - cw * 2, map.Width, 8 + cw * 2);
                if (title != null)
                    Draw.FillRectangle(Brushes.White, 0, 4, map.Width, 4 + cw * 2);
                //Draw.DrawString(message, DefFont, Brushes.Black, (map.Width - charwith) / 2, map.Height - 8 - cw * 2);
                Draw.DrawString(message, DefFont, Brushes.Black, (map.Width - charwith) / 2, map.Height - 4 - cw*2);
                cw = (int)(DefFont.Size * 32 / 48);
                charwith = 0;
                for (int i = 0; i < title.Length; i++)
                {
                    if (title[i] < 128) charwith += cw;
                    else charwith += cw * 2;
                }
                if (title != null)
                    Draw.DrawString(title, DefFont, Brushes.Black, (map.Width - charwith) / 2, 4);
            }
            Draw.Dispose();
            return map;
        }

        public static void DrawToGraphics(string str,Graphics Draw,float x,float y, float w, float h)
        {
            str=Code47toEncode(AddCheckDigits(ASCIItoCode47(str)));
            Pen p = new Pen(Color.Black, w);
            for (int i = 0; i < str.Length; i++)
                if (str[i] == '1')
                    Draw.DrawLine(p, i*w + x, y, i*w + x, y + h);
        }

        public static int getLength(string value)
        {
            return Code47toEncode(AddCheckDigits(ASCIItoCode47(value))).Length;
        }

        public static Bitmap BarCode(string str,int w,int h,string message,string title)
        {
            return ASCIItoBitmap(Code47toEncode(AddCheckDigits(ASCIItoCode47(str))), w, h,message,title);
        }

        public static Bitmap BarCode(string str,string message,string title)
        {
            string dst = Code47toEncode(AddCheckDigits(ASCIItoCode47(str)));
            return ASCIItoBitmap(dst, dst.Length * 2 + 20, 100, message,title);
        }
        public static Bitmap BarCode(string str, string message)
        {
            string dst = Code47toEncode(AddCheckDigits(ASCIItoCode47(str)));
            return ASCIItoBitmap(dst, dst.Length * 2 + 20, 100, message,null);
        }
        public static Bitmap BarCode(string str)
        {
            string dst = Code47toEncode(AddCheckDigits(ASCIItoCode47(str)));
            return ASCIItoBitmap(dst, dst.Length, 100,null,null);
        }

        public static string test(string test)
        {
            return Code47toEncode(AddCheckDigits(ASCIItoCode47(test)));
        }

    }
}
