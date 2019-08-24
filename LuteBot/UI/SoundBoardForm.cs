using LuteBot.Config;
using LuteBot.Soundboard;
using LuteBot.UI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot
{
    public partial class SoundBoardForm : Form
    {
        private String unassignedButtonString = "[Unassigned]";
        private String waitingForKeyPressString = "[Enter a Key]";
        private SoundBoardManager soundBoardManager;

        private List<Button> soundTrackButtons;
        private List<ContextMenu> soundBoardContextMenus;

        private int waitingForKeyPressIndex;

        public SoundBoardForm(SoundBoardManager soundBoardManager)
        {
            InitializeComponent();
            this.soundBoardManager = soundBoardManager;
            var lastSoundBoardPath = ConfigManager.GetProperty(PropertyItem.LastSoundBoardLocation);
            if (lastSoundBoardPath != null && lastSoundBoardPath.Length > 0)
            {
                soundBoardManager.LoadLastSoundBoard(lastSoundBoardPath);
            }
            InitButtonList();
            ContextMenuHelper();
            waitingForKeyPressIndex = -1;
            RefreshButtons();
        }

        private void InitButtonList()
        {
            soundTrackButtons = new List<Button>();
            soundTrackButtons.Add(SoundBoardButton1);
            soundTrackButtons.Add(SoundBoardButton2);
            soundTrackButtons.Add(SoundBoardButton3);
            soundTrackButtons.Add(SoundBoardButton4);
            soundTrackButtons.Add(SoundBoardButton5);
            soundTrackButtons.Add(SoundBoardButton6);
            soundTrackButtons.Add(SoundBoardButton7);
            soundTrackButtons.Add(SoundBoardButton8);
            soundTrackButtons.Add(SoundBoardButton9);
        }

        private void RefreshButtons()
        {
            for (int i = 0; i < soundTrackButtons.Count; i++)
            {
                if (i == waitingForKeyPressIndex)
                {
                    if (soundBoardManager.IsTrackAssigned(i))
                    {
                        if (soundBoardManager.GetTrack(i).Name == null || soundBoardManager.GetTrack(i).Name == "")
                        {
                            soundTrackButtons[i].Text = unassignedButtonString + "\n" + waitingForKeyPressString;
                        }
                        else
                        {
                            soundTrackButtons[i].Text = soundBoardManager.GetTrack(i).Name + "\n" + waitingForKeyPressString;
                        }
                    }
                    else
                    {
                        soundTrackButtons[i].Text = unassignedButtonString + "\n" + waitingForKeyPressString;
                    }
                }
                else
                {
                    if (soundBoardManager.IsTrackAssigned(i))
                    {
                        if (soundBoardManager.GetTrack(i).Name == null || soundBoardManager.GetTrack(i).Name == "")
                        {
                            soundTrackButtons[i].Text = unassignedButtonString + "\n" + soundBoardManager.GetTrack(i).Hotkey;
                        }
                        else
                        {
                            soundTrackButtons[i].Text = soundBoardManager.GetTrack(i).Name + "\n" + soundBoardManager.GetTrack(i).Hotkey;
                        }
                    }
                    else
                    {
                        soundTrackButtons[i].Text = unassignedButtonString;
                    }
                }
            }
        }

        private void ContextMenuHelper()
        {
            soundBoardContextMenus = new List<ContextMenu>();
            ContextMenu soundBoardContextMenu;
            for (int i = 0; i < soundTrackButtons.Count; i++)
            {
                soundBoardContextMenu = new ContextMenu();
                if (soundBoardManager.IsTrackAssigned(i))
                {
                    MenuItem unassign = soundBoardContextMenu.MenuItems.Add("Unassign");
                    unassign.Click += new EventHandler(UnassignMenuItem_Click);
                    soundBoardContextMenu.MenuItems.Add("-");
                }
                MenuItem assignTrack = soundBoardContextMenu.MenuItems.Add("Set Music file");
                assignTrack.Click += new EventHandler(AssignTrackMenuItem_Click);
                MenuItem assignHotkey = soundBoardContextMenu.MenuItems.Add("Set Hotkey");
                assignHotkey.Click += new EventHandler(AssignHotkeyMenuItem_Click);
                soundTrackButtons[i].ContextMenu = soundBoardContextMenu;
                soundBoardContextMenus.Add(soundBoardContextMenu);
            }
        }

        private void UnassignMenuItem_Click(object sender, EventArgs e)
        {
            int index = -1;
            for (int i = 0; i < soundBoardContextMenus.Count; i++)
            {
                if (sender.Equals(soundBoardContextMenus[i].MenuItems[0]))
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                soundBoardManager.DeleteTrack(index);
            }
            RefreshButtons();
        }

        private void AssignTrackMenuItem_Click(object sender, EventArgs e)
        {
            int index = -1;
            int menuItemIndex;
            for (int i = 0; i < soundBoardContextMenus.Count; i++)
            {
                menuItemIndex = 0;
                if (soundBoardContextMenus[i].MenuItems.Count == 4)
                {
                    menuItemIndex = 2;
                }
                if (sender.Equals(soundBoardContextMenus[i].MenuItems[menuItemIndex]))
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                OpenFileDialog openMidiFileDialog = new OpenFileDialog();
                openMidiFileDialog.DefaultExt = "mid";
                openMidiFileDialog.Filter = "MIDI files|*.mid|All files|*.*";
                openMidiFileDialog.Title = "Open MIDI file";
                if (openMidiFileDialog.ShowDialog() == DialogResult.OK)
                {
                    SoundBoardItem soundBoardItem;
                    if (soundBoardManager.IsTrackAssigned(index))
                    {
                        soundBoardItem = soundBoardManager.GetTrack(index);
                    }
                    else
                    {
                        soundBoardItem = new SoundBoardItem();
                    }
                    string fileName = openMidiFileDialog.FileName;
                    string filteredFileName = fileName;
                    if (fileName.Contains("\\"))
                    {
                        string[] fileNameSplit = fileName.Split('\\');
                        filteredFileName = fileNameSplit[fileNameSplit.Length - 1].Replace(".mid", "");
                    }
                    soundBoardItem.Path = fileName;
                    soundBoardItem.Name = filteredFileName;
                    soundBoardManager.SetTrack(soundBoardItem, index);
                }
            }
            RefreshButtons();
        }
        private void AssignHotkeyMenuItem_Click(object sender, EventArgs e)
        {
            int index = -1;
            int menuItemIndex;
            for (int i = 0; i < soundBoardContextMenus.Count; i++)
            {
                menuItemIndex = 1;
                if (soundBoardContextMenus[i].MenuItems.Count == 4)
                {
                    menuItemIndex = 3;
                }
                object test = soundBoardContextMenus[i].MenuItems[menuItemIndex];
                if (sender.Equals(soundBoardContextMenus[i].MenuItems[menuItemIndex]))
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                waitingForKeyPressIndex = index;
                RefreshButtons();
            }
        }


        private void SoundBoardForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (waitingForKeyPressIndex >= 0 && waitingForKeyPressIndex < soundTrackButtons.Count)
            {
                SoundBoardItem soundBoardItem;
                if (soundBoardManager.IsTrackAssigned(waitingForKeyPressIndex))
                {
                    soundBoardItem = soundBoardManager.GetTrack(waitingForKeyPressIndex);
                }
                else
                {
                    soundBoardItem = new SoundBoardItem();
                }
                soundBoardItem.Hotkey = e.KeyCode;
                soundBoardManager.SetTrack(soundBoardItem, waitingForKeyPressIndex);
                ResetWaitingForKeyPress();
            }
        }

        private void SoundBoardButton_MouseDown(object sender, MouseEventArgs e)
        {
            ContextMenuHelper();
        }

        private void ResetWaitingForKeyPress()
        {
            waitingForKeyPressIndex = -1;
            RefreshButtons();
            ContextMenuHelper();
        }

        private void SoundBoardForm_Click(object sender, EventArgs e)
        {
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton1_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(0);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton2_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(1);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton3_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(2);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton4_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(3);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton5_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(4);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton6_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(5);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton7_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(6);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton8_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(7);
            ResetWaitingForKeyPress();
        }

        private void SoundBoardButton9_Click(object sender, EventArgs e)
        {
            soundBoardManager.PlayTrack(8);
            ResetWaitingForKeyPress();
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {
            soundBoardManager.Load();
            RefreshButtons();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            soundBoardManager.Save();
        }

        private void SoundBoardForm_Closing(object sender, FormClosingEventArgs e)
        {
            ConfigManager.SetProperty(PropertyItem.LastSoundBoardLocation, soundBoardManager.LastSoundBoardLocation);
            WindowPositionUtils.UpdateBounds(PropertyItem.SoundBoardPos, new Point() { X = Left, Y = Top });
            ConfigManager.SaveConfig();
            soundBoardManager.Dispose();
        }
    }
}