using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace com.github.KeyMove.Tools
{
    class WindowTools
    {
        public static void IntInput(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (!(e.KeyChar >= '0' && e.KeyChar <= '9'))
                {
                    if (e.KeyChar != 8)
                        e.Handled = true;
                }
            }
            catch { }
        }
    }
}
