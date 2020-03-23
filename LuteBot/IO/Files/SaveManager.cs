using LuteBot.playlist;
using LuteBot.Playlist;
using LuteBot.Soundboard;
using LuteBot.TrackSelection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.IO.Files
{
    public class SaveManager
    {
        private static string autoSavePath = "Profiles";

        private static bool CheckIfFileExists(string path)
        {
            bool fileNameAvaliable = false;
            if (File.Exists(path))
            {
                DialogResult dialogResult = MessageBox.Show("File already exists\nDo you want to replace it", "Save PlayList", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    File.Delete(path);
                    fileNameAvaliable = true;
                }
            }
            else
            {
                fileNameAvaliable = true;
            }
            return fileNameAvaliable;
        }

        public static void SavePlayList(PlayList musicList)
        {
            Save(musicList);
        }

        public static PlayList LoadPlayList()
        {
            return Load<PlayList>();
        }

        public static PlayList LoadLastPlayList(string path)
        {
            return LoadNoDialog<PlayList>(path);
        }

        public static void SaveSoundBoard(SoundBoard soundBoard)
        {
            Save(soundBoard);
        }

        public static SoundBoard LoadSoundBoard()
        {
            return Load<SoundBoard>();
        }

        public static SoundBoard LoadLastSoundBoard(string path)
        {
            return LoadNoDialog<SoundBoard>(path);
        }

        public static void SaveTrackSelectionData(TrackSelectionData data, string fileName)
        {
            SaveNoDialog(data, autoSavePath + "/" + fileName);
        }

        public static TrackSelectionData LoadTrackSelectionData(string fileName)
        {
            return LoadNoDialog<TrackSelectionData>(autoSavePath + "/" + fileName);
        }

        public static string SetMordhauConfigLocation()
        {
            string fileName = null;
            while (fileName == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "ini";
                openFileDialog.Filter = "INI files|*.ini";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = openFileDialog.FileName;
                    if (!fileName.Contains("DefaultInput.ini"))
                    {
                        MessageBox.Show("Please select the file \"DefaultInput.ini\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        fileName = null;
                    }
                }
                else
                {
                    fileName = "";
                }
            }
            return fileName;
        }

        public static void SaveMordhauConfig(string fileName, string fileContent)
        {
            using (var stream = File.Create(fileName))
            {
                stream.Write(Encoding.UTF8.GetBytes(fileContent), 0, fileContent.Length);
            }
        }

        public static string LoadMordhauConfig(string fileName)
        {
            byte[] streamResult;
            string content;

            try
            {
                using (var stream = File.Open(fileName, FileMode.Open))
                {
                    streamResult = new byte[stream.Length];
                    stream.Read(streamResult, 0, (int)stream.Length);
                    content = Encoding.UTF8.GetString(streamResult);
                }
            }
            catch (Exception)
            {
                content = null;
            }
            return content;
        }

        private static T LoadNoDialog<T>(string path)
        {
            T result = default(T);
            if (path != null)
            {

                if (!path.Contains(".xml"))
                {
                    path = path + ".xml";
                }
                Directory.CreateDirectory(autoSavePath);
                if (File.Exists(path))
                {
                    using (var stream = File.Open(path, FileMode.OpenOrCreate))
                    {
                        var serializer = new DataContractSerializer(typeof(T));
                        try
                        {
                            result = (T)serializer.ReadObject(stream);
                        }
                        catch (InvalidCastException)
                        {
                            MessageBox.Show("Wrong File type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            return result;
        }

        private static void SaveNoDialog<T>(T target, string path)
        {
            if (path != null)
            {
                path = path + ".xml";
                Directory.CreateDirectory(autoSavePath);
                using (var stream = File.Create(path))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(stream, target);
                }
            }
        }

        private static void Save<T>(T target)
        {
            string path = SaveFileDialogHelper();
            if (path != null)
            {
                //dirty !!
                if (typeof(T) == typeof(SoundBoard))
                {
                    (target as SoundBoard).Location = path;
                }
                if (typeof(T) == typeof(PlayList))
                {
                    (target as PlayList).Path = path;
                }
                using (var stream = File.Create(path))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    serializer.WriteObject(stream, target);
                }
            }
        }

        private static T Load<T>()
        {
            T result = default(T);
            string path = LoadFileDialogHelper();
            if (path != null)
            {
                using (var stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    var serializer = new DataContractSerializer(typeof(T));
                    try
                    {
                        result = (T)serializer.ReadObject(stream);
                        //dirty !!
                        if (typeof(T) == typeof(SoundBoard))
                        {
                            (result as SoundBoard).Location = path;
                        }
                        if (typeof(T) == typeof(PlayList))
                        {
                            (result as PlayList).Path = path;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Wrong File type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return result;
        }

        private static string LoadFileDialogHelper()
        {
            string result = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = "xml";
            openFileDialog.Filter = "XML files|*.xml";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                result = openFileDialog.FileName;
            }
            return result;
        }

        private static string SaveFileDialogHelper()
        {
            string result = null;
            SaveFileDialog savefile = new SaveFileDialog();
            savefile.Filter = "XML files |*.xml";
            savefile.DefaultExt = "xml";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                result = (savefile.FileName);
            }
            return result;
        }

        private static string BuildURL(params string[] paths)
        {
            string result = "";
            foreach (string path in paths)
            {
                result = result + path + "/";
            }
            result.Remove(result.Length - 1, 1);
            return result;
        }
    }
}
