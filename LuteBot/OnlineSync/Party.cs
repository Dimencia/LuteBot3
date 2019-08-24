using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.OnlineSync
{
    public class Party
    {
        public const int MaxMembers = 10;

        private PlayerProfile host;
        private List<PlayerProfile> clients;

        public PlayerProfile Host { get => host;}

        public Party(PlayerProfile host)
        {
            clients = new List<PlayerProfile>();
            this.host = host;
        }
    }
}
