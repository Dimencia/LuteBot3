using LuteBot.Config;
using LuteBot.IO.KB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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
        private string serverIP = null;

        private int lastNtpPing = 20;
        private int lastServerPing = 0;

        private const string ntpServer = "time.nist.gov";

        public TimeSyncForm(LuteBotForm parent)
        {
            this.parent = parent;
            InitializeComponent();


            serverIP = ConfigManager.GetProperty(PropertyItem.Server);
            serverBox.Text = serverIP;
            // Run a timer to check system time every 10ms or maybe faster
            System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();
            t.Interval = 10;
            t.Tick += T_Tick;
            t.Enabled = true;
            t.Start();
            // We'll also update our server time every second or so
            System.Windows.Forms.Timer serverTimer = new System.Windows.Forms.Timer();
            serverTimer.Interval = 5000;
            serverTimer.Tick += ServerTimer_Tick;
            serverTimer.Enabled = true;
            serverTimer.Start();

            System.Windows.Forms.Timer pingTimer = new System.Windows.Forms.Timer();
            pingTimer.Interval = 1000;
            pingTimer.Tick += PingTimer_Tick;
            pingTimer.Enabled = true;
            pingTimer.Start();

            dateTimeOffsetMS = (int)(DateTime.UtcNow - GetNetworkTime()).TotalMilliseconds; // Set initial value
        }

        public void StartAtNextInterval(int interval)
        {
            // Waits at least minSeconds, and starts at the first available time that's divisible by interval seconds
            Task.Run(() =>
            {
                
                var startTime = (DateTime.UtcNow - TimeSpan.FromMilliseconds(dateTimeOffsetMS));
                var curTime = startTime;
                DateTime targetTime = curTime.AddSeconds(interval - (curTime.Second % interval));
                targetTime = targetTime.AddMilliseconds(-1 * targetTime.Millisecond);

                Invoke((MethodInvoker)delegate
                {
                    button1.Enabled = false;
                    inputTime.Enabled = false;
                    button1.Text = "Waiting...";
                    inputTime.Text = $"{targetTime.Hour}:{targetTime.Minute}:{targetTime.Second}.{targetTime.Millisecond}";
                });

                while ((curTime < targetTime))
                {
                    Thread.Sleep(10);
                    curTime = (DateTime.UtcNow - TimeSpan.FromMilliseconds(dateTimeOffsetMS));
                }
                Invoke((MethodInvoker)delegate
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

        private void PingTimer_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                Ping pinger = new Ping();
                int timeout = 250; // More than 1000 and we start overlap, and that's bad.

                var result = pinger.Send(ntpServer, timeout);
                // We get timestamp from when it sent, so we only want half the roundtrip time
                if (result.Status == IPStatus.Success)
                {
                    lastNtpPing = (int)result.RoundtripTime / 2;
                }

                string server = ConfigManager.GetProperty(PropertyItem.Server);

                if (server != null)
                {
                    try
                    {
                        result = pinger.Send(server, timeout);

                        // And now for the server
                        if (result.Status == IPStatus.Success)
                        {
                            lastServerPing = (int)result.RoundtripTime / 2; // Again, one way is all we want
                        }
                    }
                    catch (Exception ex) { lastServerPing = 0; }
                }

            });
        }

        private void ServerTimer_Tick(object sender, EventArgs e)
        {
            dateTimeOffsetMS = (int)(DateTime.UtcNow - GetNetworkTime()).TotalMilliseconds;
        }

        public int dateTimeOffsetMS = 0;

        private void UpdateTime()
        {
            if (!this.IsDisposed)
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
            if (!this.IsDisposed)
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
                while ((DateTime.UtcNow - TimeSpan.FromMilliseconds(dateTimeOffsetMS)) < targetTime)
                {
                    Thread.Sleep(10);
                }
                Invoke((MethodInvoker)delegate
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


        public DateTime GetNetworkTime()
        {

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry(ntpServer).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP

            using (var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp))
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

            milliseconds += (ulong)lastNtpPing;
            ntpPingLabel.Text = lastNtpPing.ToString();

            milliseconds += (ulong)lastServerPing;
            serverPingLabel.Text = lastServerPing.ToString();


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

        private void ServerBox_TextChanged(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.Server, serverBox.Text);
        }
    }
}
