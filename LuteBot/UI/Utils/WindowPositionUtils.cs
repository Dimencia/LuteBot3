using LuteBot.Config;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI.Utils
{
    public class WindowPositionUtils
    {
        public static void UpdateBounds(PropertyItem item, Point bounds)
        {
            ConfigManager.SetProperty(item, $"{bounds.X}|{bounds.Y}");
        }

        public static Point CheckPosition(Point position)
        {
            if ((position.X == 0 && position.Y == 0) || position.X > Screen.PrimaryScreen.Bounds.Width || position.Y > Screen.PrimaryScreen.Bounds.Height)
            {
                return new Point() { X = Screen.PrimaryScreen.Bounds.Width / 2, Y = Screen.PrimaryScreen.Bounds.Height / 2 };
            }
            return position;
        }

    }
}