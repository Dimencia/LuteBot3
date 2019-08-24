using LuteBot.IO.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Soundboard
{
    public class SoundBoardManager
    {
        private SoundBoard soundBoard;

        public string LastSoundBoardLocation { get => soundBoard.Location; }

        public event EventHandler<SoundBoardEventArgs> SoundBoardTrackRequest;

        public SoundBoardManager()
        {
            soundBoard = new SoundBoard();
            soundBoard.SoundBoardItems = new SoundBoardItem[9];
        }

        public void Save()
        {
            SaveManager.SaveSoundBoard(soundBoard);
        }

        public void Load()
        {
            SoundBoard tempSoundBoard = SaveManager.LoadSoundBoard();
            if (tempSoundBoard != null)
            {
                soundBoard = tempSoundBoard;
            }
            else
            {
                soundBoard = new SoundBoard();
                soundBoard.SoundBoardItems = new SoundBoardItem[9];
            }
        }

        public void Dispose()
        {
            soundBoard = new SoundBoard();
            soundBoard.SoundBoardItems = new SoundBoardItem[9];
        }

        public SoundBoardManager(int size)
        {
            soundBoard = new SoundBoard();
            soundBoard.SoundBoardItems = new SoundBoardItem[size];
        }

        public bool IsTrackAssigned(int index)
        {
            bool result = false;
            if (index >= 0 && index < soundBoard.SoundBoardItems.Length)
            {
                result = soundBoard.SoundBoardItems[index] != null;
            }
            return result;
        }

        public void LoadLastSoundBoard(string lastSoundBoardPath)
        {
            SoundBoard tempSoundBoard = SaveManager.LoadLastSoundBoard(lastSoundBoardPath);
            if (tempSoundBoard != null)
            {
                soundBoard = tempSoundBoard;
            }
            else
            {
                soundBoard = new SoundBoard();
                soundBoard.SoundBoardItems = new SoundBoardItem[9];
            }
        }

        public void KeyPressed(Keys key)
        {
            for (int i = 0; i < soundBoard.SoundBoardItems.Length; i++)
            {
                if (soundBoard.SoundBoardItems[i] != null && soundBoard.SoundBoardItems[i].Hotkey == key)
                {
                    PlayTrack(i);
                    break;
                }
            }
        }

        public void PlayTrack(int index)
        {
            if (index >= 0 && index < soundBoard.SoundBoardItems.Length && soundBoard.SoundBoardItems[index] != null && soundBoard.SoundBoardItems[index].Path != null && soundBoard.SoundBoardItems[index].Path != "")
            {
                EventHelper(index);
            }
        }

        public void SetTrack(SoundBoardItem item, int index)
        {
            if (index >= 0 && index < soundBoard.SoundBoardItems.Length)
            {
                soundBoard.SoundBoardItems[index] = item;
            }
        }

        public SoundBoardItem GetTrack(int index)
        {

            SoundBoardItem newItem = new SoundBoardItem()
            {
                Hotkey = soundBoard.SoundBoardItems[index].Hotkey,
                Path = soundBoard.SoundBoardItems[index].Path,
                Name = soundBoard.SoundBoardItems[index].Name
            };
            return newItem;
        }

        public List<SoundBoardItem> GetTrackList()
        {
            List<SoundBoardItem> trackList = new List<SoundBoardItem>();

            foreach (SoundBoardItem item in soundBoard.SoundBoardItems)
            {
                SoundBoardItem newItem = new SoundBoardItem()
                {
                    Hotkey = item.Hotkey,
                    Path = item.Path,
                    Name = item.Name
                };
                trackList.Add(newItem);
            }

            return trackList;
        }

        public void DeleteTrack(int index)
        {
            soundBoard.SoundBoardItems[index] = null;
        }

        private void EventHelper(int index)
        {
            EventHandler<SoundBoardEventArgs> handler = SoundBoardTrackRequest;
            SoundBoardEventArgs eventArgs = new SoundBoardEventArgs();
            eventArgs.SelectedTrack = soundBoard.SoundBoardItems[index];
            handler?.Invoke(this, eventArgs);
        }
    }
}
