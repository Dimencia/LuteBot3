using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI.Utils
{
    class CustomListBox : ListBox
    {
        private const int WM_LBUTTONDOWN = 0x201;

        public event EventHandler PreSelect;

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LBUTTONDOWN:
                    OnPreSelect();
                    break;
            }

            base.WndProc(ref m);
        }

        protected void OnPreSelect()
        {
            if (null != PreSelect)
                PreSelect(this, new EventArgs());
        }
    }
}
