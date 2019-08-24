using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.OnlineSync
{
    public class PlayerProfile
    {
        private string name;
        private string ipAdress;

        private bool isReady;

        public string Name { get => name; set => name = value; }
        public string IpAdress { get => ipAdress; set => ipAdress = value; }
        public bool IsReady { get => isReady; set => isReady = value; }
    }
}
