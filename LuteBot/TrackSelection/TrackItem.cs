using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class TrackItem
    {
        private string name;
        private bool active;
        private int id;

        public string Name { get => name; set => name = value; }
        public bool Active { get => active; set => active = value; }
        public int Id { get => id; set => id = value; }
    }
}
