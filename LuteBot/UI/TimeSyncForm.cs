using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI
{
    public partial class TimeSyncForm : Form
    {
        private LuteBotForm parent;
        public TimeSyncForm(LuteBotForm parent)
        {
            this.parent = parent;
            InitializeComponent();
            // Run a timer to check system time every 10ms or maybe faster
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 10;
            t.Tick += T_Tick;
            t.Enabled = true;
            t.Start();
            // We'll also update our server time every second or so
            System.Windows.Forms.Timer serverTimer = new System.Windows.Forms.Timer();
            serverTimer.Interval = 1000;
            serverTimer.Tick += ServerTimer_Tick;
            serverTimer.Enabled = true;
            serverTimer.Start();
            dateTimeOffsetMS = (int)(DateTime.UtcNow - GetNetworkTime()).TotalMilliseconds; // Set initial value
        }

        private void ServerTimer_Tick(object sender, EventArgs e)
        {
            dateTimeOffsetMS = (int)(DateTime.UtcNow - GetNetworkTime()).TotalMilliseconds;
        }

        public int dateTimeOffsetMS = 0;

        private void UpdateTime()
        {
            if(!this.IsDisposed)
            {
                string timeText = (DateTime.UtcNow - TimeSpan.FromMilliseconds(dateTimeOffsetMS)).ToString("HH:mm:ss.FFF");
                if (InvokeRequired)
                    Invoke((MethodInvoker)delegate
                    {
                        ServerTimeLabel.Text = timeText;
                    });
                else
                    ServerTimeLabel.Text = timeText;
            }
        }

        private void T_Tick(object sender, EventArgs e)
        {
            if(!this.IsDisposed)
                UpdateTime();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            // So get the value in the inputTime box, wait until adjusted system time is as close to it as possible
            // Basically loop, checking system time, and if targetTime-adjustedTime becomes 0 or positive, trigger
            DateTime targetTime = new DateTime();
            try
            {
                targetTime = DateTime.Parse(inputTime.Text);
            }
            catch (Exception)
            {
                // no way to report exceptions...
                return;
            }

            button1.Enabled = false;
            inputTime.Enabled = false;
            button1.Text = "Waiting...";
            Task.Run(() =>
            {
                while((DateTime.UtcNow - TimeSpan.FromMilliseconds(dateTimeOffsetMS)) < targetTime)
                {
                    Thread.Sleep(10);
                }
                Invoke((MethodInvoker) delegate
                {
                    button1.Enabled = true;
                    button1.Text = "Set";
                    inputTime.Enabled = true;
                });
                // Trigger it to start playing somehow
                parent.Invoke((MethodInvoker)delegate
               {
                   parent.Play();
               });
            });
        }


        public static DateTime GetNetworkTime()
        {
            //default Windows time server
            const string ntpServer = "time.windows.com";

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);

                //Stops code hang if NTP is blocked
                socket.ReceiveTimeout = 3000;

                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Close();
            }

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);

            return networkDateTime;
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}
