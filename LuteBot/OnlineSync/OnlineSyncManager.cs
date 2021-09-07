using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.OnlineSync
{
    public class OnlineSyncManager
    {
        public enum OnlineStatus
        {
            Offline,
            Host,
            Client
        }

        private event EventHandler Disconnected;
        private event EventHandler Connected;
        private event EventHandler ReadyStatusChanged;

        private OnlineStatus status;

        private Party currentLobby;

        private PlayerProfile userProfile;

        private AsynchronousSocketListener server;
        private AsynchronousClient client;

        public OnlineStatus Status { get => status; }
        public PlayerProfile UserProfile { get => userProfile; set => userProfile = value; }

        public OnlineSyncManager()
        {
            UserProfile = new PlayerProfile() { IpAdress = GetIP(), IsReady = false };
            status = OnlineStatus.Offline;
        }

        public static bool IsValidIpAdress(string input)
        {
            bool result = false;
            if (IPAddress.TryParse(input, out IPAddress address))
            {
                switch (address.AddressFamily)
                {
                    case System.Net.Sockets.AddressFamily.InterNetwork:
                        result = true;
                        break;
                    case System.Net.Sockets.AddressFamily.InterNetworkV6:
                        result = true;
                        break;
                    default:
                        result = false;
                        break;
                }
            }
            return result;
        }

        private string GetIP()
        {
            try
            {
                string url = "http://checkip.dyndns.org";
                System.Net.WebRequest req = System.Net.WebRequest.Create(url);
                System.Net.WebResponse resp = req.GetResponse();
                System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
                string response = sr.ReadToEnd().Trim();
                string[] a = response.Split(':');
                string a2 = a[1].Substring(1);
                string[] a3 = a2.Split('<');
                string a4 = a3[0];
                return a4;
            }
            catch
            {
                return "0.0.0.0";
            }
        }

        public string GetCurrentLobbyIp()
        {
            return currentLobby.Host.IpAdress;
        }

        public void CreateParty()
        {
            this.currentLobby = new Party(UserProfile);
            server = new AsynchronousSocketListener();
            client = null;
            server.StartListening(userProfile.IpAdress);
        }

        public void JoinParty(string ip)
        {
            
        }

        public void ToggleReady()
        {
            UserProfile.IsReady = !UserProfile.IsReady;
        }

        public void LeaveParty()
        {
            if (status == OnlineStatus.Client)
            {

            } else if (status == OnlineStatus.Host)
            {

            }
        }
    }
}
