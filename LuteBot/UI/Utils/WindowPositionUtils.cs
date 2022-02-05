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
            var targetScreen = Screen.FromPoint(position);
            
            if ((position.X == 0 && position.Y == 0) || !targetScreen.Bounds.Contains(position))
            {
                return new Point() { X = targetScreen.Bounds.X + targetScreen.Bounds.Width / 2, Y = targetScreen.Bounds.Y + targetScreen.Bounds.Height / 2 };
            }
            return position;
        }

    }
}