using LuteBot.OnlineSync;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.UI
{
    public partial class OnlineSyncForm : Form
    {
        OnlineSyncManager onlineManager;
        public OnlineSyncForm(OnlineSyncManager onlineManager)
        {
            InitializeComponent();
            this.onlineManager = onlineManager;
            UpdateComponents();
        }

        private void UpdateComponents()
        {
            OnlineSyncManager.OnlineStatus status = onlineManager.Status;

            if (status == OnlineSyncManager.OnlineStatus.Offline)
            {
                NameTextBox.Enabled = true;
                ReadyButton.Enabled = false;
                RoomIpAdressLabel.Text = onlineManager.UserProfile.IpAdress;
                ConnectButton.Text = "Connect To Lobby";
                IpTextBox.Enabled = true;
            }
            if (status == OnlineSyncManager.OnlineStatus.Client)
            {
                NameTextBox.Enabled = false;
                ReadyButton.Enabled = true;
                ConnectButton.Text = "Disconnect From Lobby";
                RoomIpAdressLabel.Text = onlineManager.GetCurrentLobbyIp();
                IpTextBox.Enabled = false;
            }
            if (status == OnlineSyncManager.OnlineStatus.Host)
            {
                NameTextBox.Enabled = false;
                ReadyButton.Enabled = false;
                RoomIpAdressLabel.Text = onlineManager.GetCurrentLobbyIp();
                ConnectButton.Text = "Disband Lobby";
                IpTextBox.Enabled = false;
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            if (OnlineSyncManager.IsValidIpAdress(IpTextBox.Text))
            {
                onlineManager.JoinParty(IpTextBox.Text);
            }
            UpdateComponents();
        }

        private void ReadyButton_Click(object sender, EventArgs e)
        {
            onlineManager.ToggleReady();
            UpdateComponents();
        }

        private void HostButton_Click(object sender, EventArgs e)
        {
            onlineManager.CreateParty();
            UpdateComponents();
        }
    }
}
