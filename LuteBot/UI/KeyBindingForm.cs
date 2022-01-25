using LuteBot.Config;
using LuteBot.IO.Files;
using LuteBot.IO.KB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    public partial class KeyBindingForm : Form
    {
        private string currentKey;

        private readonly string keyChangeString = "[input a key or ESC to cancel]";
        public KeyBindingForm()
        {
            InitializeComponent();
            InitPropertiesList();
            RefreshConfigFoundLabel();
            this.FormClosing += KeyBindingForm_FormClosing;
        }

        private void KeyBindingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void SetConfig_Click(object sender, EventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.MordhauInputIniLocation, SaveManager.SetMordhauConfigLocation());
            RefreshConfigFoundLabel();
        }

        private void RefreshConfigFoundLabel()
        {
            try
            {
                // This should basically always exist - it's a hardcoded location in localappdata.  And since our config folder has moved, they won't have an old config
                if (string.IsNullOrWhiteSpace(SaveManager.LoadMordhauConfig(ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation))))
                {
                    MordhauConfigLabel.Text = "Mordhau configuration file not found. Please set the location of Input.ini in your config file";
                }
                else
                {
                    MordhauConfigLabel.Text = "Mordhau configuration file found";
                }
            }
            catch (Exception ex)
            {
                MordhauConfigLabel.Text = "Error while loading Mordhau config file : " + ex.Message;
            }
        }

        public void InitPropertiesList()
        {
            HotkeysList.Items.Clear();
            var hotkeys = Enum.GetValues(typeof(Keybinds));

            String[] row = new String[2];
            foreach (Keybinds hotkey in hotkeys)
            {
                row[0] = hotkey.ToString();
                row[1] = ConfigManager.GetProperty((PropertyItem)hotkey);
                HotkeysList.Items.Add(new ListViewItem(row));
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            ConfigManager.Refresh();
            this.Close();
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            ConfigManager.SaveConfig();
            LuteBotForm.SetConsoleKey(true, true); // Quiet if already set, force mordhau to update if not
            this.Close();
        }

        private void KeyHandler(object sender, KeyEventArgs e)
        {
            Keys tempKey;
            if (e.KeyCode == Keys.Escape)
            {
                if (HotkeysList.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = HotkeysList.SelectedItems[0];
                    selectedItem.SubItems[1].Text = currentKey;
                }
            }
            else
            {
                if (HotkeysList.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = HotkeysList.SelectedItems[0];
                    Enum.TryParse<Keys>(e.KeyCode.ToString(), out tempKey);
                    selectedItem.SubItems[1].Text = tempKey.ToString();
                    ConfigManager.SetProperty(Property.FromString(selectedItem.SubItems[0].Text), selectedItem.SubItems[1].Text);
                }
            }
            ToggleEnableLists(true);
        }

        private void HotkeysList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (HotkeysList.SelectedItems.Count > 0)
            {
                ToggleEnableLists(false);
                ListViewItem selectedItem = HotkeysList.SelectedItems[0];
                currentKey = selectedItem.SubItems[1].Text;
                selectedItem.SubItems[1].Text = keyChangeString;
            }
        }

        private void ToggleEnableLists(bool enabled)
        {
            HotkeysList.Enabled = enabled;
        }

        private void RevertAutoConfig_Click(object sender, EventArgs e)
        {
            ConfigManager.Refresh();
            string configLocation = ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation);
            string configContent = SaveManager.LoadMordhauConfig(configLocation);
            if (configContent.Contains("+ConsoleKeys=PageDown"))
            {
                configContent = configContent.Replace("+ConsoleKeys=PageDown", "");
                SaveManager.SaveMordhauConfig(configLocation, configContent);
            }
            ConfigManager.SetProperty(PropertyItem.OpenConsole, ConfigManager.GetProperty(PropertyItem.UserSavedConsoleKey));
            InitPropertiesList();
        }
        private void OpenConfig_Click(object sender, EventArgs e)
        {
            string path = ConfigManager.GetProperty(PropertyItem.MordhauInputIniLocation);
            string cmd = "explorer.exe";
            string arg = "/select, " + path;
            Process.Start(cmd, arg);
        }
    }
}
