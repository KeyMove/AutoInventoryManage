using com.github.KeyMove.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoInvtoryManage
{
    interface BarCodePrint
    {
        void SetPageSize(int w,int h);
        void Draw(Graphics g, float x, float y, string value, string title, string message);
        void Draw(Graphics g, int index, string value, string title, string message);
        Bitmap getImage(string value, string title, string message);
    }

    class DefBarCode:BarCodePrint
    {
        float startx;
        float starty;
        int col;
        int row;
        int maxindex;
        Font deffont;
        public DefBarCode(int w,int h,Font f=null)
        {
            SetPageSize(w, h);
            deffont =f!=null? f : new Font("宋体", 9);
        }

        public DefBarCode()
        {
            SetPageSize(210, 297);
            deffont = new Font("宋体", 9);
        }
        
        float toCent(string str)
        {
            float fh = deffont.Size * 32 / 24 / 4;
            float len = 0;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] > 128)
                    len += fh;
                else
                    len += fh / 2;
            }
            return (29 - len) / 2;
        }

        public void Draw(Graphics g, int index,string value,string title,string message)
        {
            if (index >= maxindex) return;
            float x = startx + 32 * (index % col);
            float y = starty + 22 * (index / col);
            Draw(g, x, y, value, title, message);
        }

        public void Draw(Graphics g, float x, float y, string value, string title, string message)
        {
            Pen p = new Pen(Color.Black, 0.5f);
            g.DrawRectangle(p, x, y, 30 - 1, 20 - 1);
            float codelen = Code93.getLength(value) * 0.15f;
            codelen = (29 - codelen) / 2;
            float fh = deffont.Size * 32 / 24 / 4;
            Code93.DrawToGraphics(value, g, codelen + x, y + 5, 0.15f, 8);
            g.DrawString(message, deffont, Brushes.Black, x + toCent(message), y + 1);
            g.DrawString(title, deffont, Brushes.Black, x + toCent(title), y + 19 - 4);
        }

        public Bitmap getImage(string value, string title, string message)
        {
            Bitmap b = new Bitmap(30, 20);
            Graphics g = Graphics.FromImage(b);
            Draw(g, 0, 0, value, title, message);
            g.Dispose();
            return b;
        }

        public void SetPageSize(int w,int h)
        {
            col = (w - 8) / 32;
            row = (h - 8) / 22;
            startx = (w - (col * 32)) / 2;
            starty = (h - (row * 22)) / 2;
            maxindex = col * row;
        }
    }
}
