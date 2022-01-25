using LuteBot.playlist;
using LuteBot.Playlist;
using LuteBot.Soundboard;
using LuteBot.TrackSelection;
using LuteBot.UI.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace LuteBot.IO.Files
{
    public class SaveManager
    {
        private static string autoSavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"LuteBot","Profiles");

        private static int fileSize = 5000;
        private static List<byte> fileHeader;
        private static List<byte> fileEnd;

        public static readonly string SaveFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\..\Local\Mordhau\Saved\SaveGames\";

        public static string ReadSavFile(string filePath)
        {
            StringBuilder strbld = new StringBuilder();
            bool fileFound = true;
            string temp;
            int i = 0;

            while (fileFound)
            {
                temp = GetDataFromFile(filePath + "[" + i + "].sav");
                fileFound = temp != null;
                strbld.Append(temp);
                i++;
            }

            return strbld.ToString();
        }

        public static void WriteSaveFile(string filePath, string content)
        {
            int i = 0;
            while (SaveDataInFile(filePath, content, i))
            {
                i++;
            }
        }

        public static void DeleteData(string filePath)
        {
            int i = 0;
            while (FileIO.DeleteFile(filePath + "[" + i + "].sav"))
            {
                i++;
            }
        }

        private static string GetDataFromFile(string filePath)
        {
            byte[] readResult = FileIO.LoadFile(filePath);
            int i;
            List<byte> retrievedData = new List<byte>();
            if (readResult == null)
            {
                return null;
            }
            else
            {
                fileHeader = new List<byte>();
                fileEnd = new List<byte>();
                i = readResult.Length - 1;
                while (i >= 0 && !(readResult[i] == 124 || readResult[i] == 64))
                {
                    i--;
                    fileEnd.Add(readResult[i]);
                }
                fileEnd.RemoveAt(fileEnd.Count - 1);
                while (i >= 0 && !(readResult[i] == 0))
                {
                    retrievedData.Add(readResult[i]);
                    i--;
                }
                while (i >= 0)
                {
                    fileHeader.Add(readResult[i]);
                    i--;
                }
                retrievedData.Reverse();
                fileEnd.Reverse();
                fileHeader.Reverse();
                fileSize = retrievedData.Count;
                string debug1 = Encoding.UTF8.GetString(fileEnd.ToArray());
                string debug2 = Encoding.UTF8.GetString(retrievedData.ToArray());
                string debug3 = Encoding.UTF8.GetString(fileHeader.ToArray());
                return Encoding.UTF8.GetString(retrievedData.ToArray()).Replace("\0", "").Replace("@", "");
            }
        }

        private static bool SaveDataInFile(string filePath, string stringData, int offset)
        {
            byte[] data = Encoding.UTF8.GetBytes(stringData);
            int i = 0;
            byte[] fileContent = new byte[fileSize];
            while (((offset * fileSize) + i < data.Length) && i < fileSize)
            {
                fileContent[i] = data[(offset * fileSize) + i];
                i++;
            }
            if (!((offset * fileSize) + i < data.Length))
            {
                while (i < fileSize)
                {
                    fileContent[i] = 64;
                    i++;
                }
            }
            SaveDataOnDisk(filePath + "[" + offset + "].sav", fileContent);
            return ((offset * fileSize) + i < data.Length);
        }

        private static void SaveDataOnDisk(string filePath, byte[] value)
        {
            byte[] data = new byte[fileHeader.Count + value.Length + fileEnd.Count];
            int i, y;
            for (i = 0; i < fileHeader.Count; i++)
            {
                data[i] = fileHeader[i];
            }
            for (y = 0; y < value.Length; i++, y++)
            {
                data[i] = value[y];
            }
            for (y = 0; y < fileEnd.Count; i++, y++)
            {
                data[i] = fileEnd[y];
            }
            FileIO.SaveFile(filePath, data);
        }


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

        public static void SaveTrackSelectionData(Dictionary<int, TrackSelectionData> data, string fileName, string targetPath = null)
        {
            SaveNoDialog(data, fileName, targetPath);
        }

        public static Dictionary<int, TrackSelectionData> LoadTrackSelectionData(string fileName)
        {
            //return LoadNoDialog<TrackSelectionData>(autoSavePath + "/" + fileName);
            // Backwards compatible in case TSD isn't there.  

            var result = LoadNoDialog<Dictionary<int, TrackSelectionData>>(fileName);
            if (result == null)
            {
                var singularData = LoadNoDialog<TrackSelectionData>(fileName);
                result = new Dictionary<int, TrackSelectionData>() { { 0, singularData } };
            }
            return result;
        }

        public static string SetMordhauConfigLocation()
        {
            string fileName = null;
            while (fileName == null)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.DefaultExt = "ini";
                openFileDialog.Filter = "INI files|*.ini";
                openFileDialog.InitialDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mordhau", "Saved", "Config", "WindowsClient");
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = openFileDialog.FileName;
                    if (!fileName.Contains("Input.ini"))
                    {
                        MessageBox.Show("Please select the file \"Input.ini\"", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
                fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mordhau", "Saved", "Config", "WindowsClient", "Input.ini");
            if (!File.Exists(fileName))
                return null;
            // If the file exists, save it into config... this doesn't really go here... oh well.
            Config.ConfigManager.SetProperty(Config.PropertyItem.MordhauInputIniLocation, fileName);
            // It's really not necessary, either.  Blank defaults to the usual one, that's good.  
            // Yeah, it's necessary because otherwise I'd have to hunt down everywhere that calls it.  Which isn't that many places but still

            try
            {
                //using (var stream = File.Open(fileName, FileMode.Open))
                //{
                //    streamResult = new byte[stream.Length];
                //    stream.Read(streamResult, 0, (int)stream.Length);
                //    content = Encoding.UTF8.GetString(streamResult);
                //}
                content = File.ReadAllText(fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                content = null;
            }
            return content;
        }

        private static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            //writer.Dispose(); // This disposes the underlying stream
            // And writer otherwise has nothing that needs disposing
            stream.Position = 0;
            return stream;
        }

        private static T LoadNoDialog<T>(string path)
        {
            T result = default(T);
            if (path != null)
            {
                var serializer = new DataContractSerializer(typeof(T));
                // Also dirty
                if (typeof(T) == typeof(TrackSelectionData) || typeof(T) == typeof(Dictionary<int, TrackSelectionData>))
                {
                    // We do things totally different here.
                    // We load from the mid file
                    // Cut out everything before the first <
                    // Then parse the rest of the content as our object
                    // If this doesn't work, we just fall through and try the default
                    try
                    {
                        using (StreamReader reader = new StreamReader(path))
                        {
                            string line = reader.ReadLine();
                            while (line != null && !line.StartsWith("<ArrayOfKeyValueOfintTrackSelectionDatatebA3aWD") && !line.StartsWith("<TrackSelectionData"))
                                line = reader.ReadLine();
                            line += "\n" + reader.ReadToEnd(); // This is our entire xml data now...
                            // Now, because of the way I read this, I can't rewind the stream
                            // So we need to make a new one with just this data
                            using (Stream xmlStream = GenerateStreamFromString(line))
                            {
                                result = (T)serializer.ReadObject(xmlStream);
                                return result; // If it fails at any point, it'll drop to old
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"LuteBot","Profiles",Path.GetFileNameWithoutExtension(path));
                    }
                }

                Directory.CreateDirectory(autoSavePath);
                path = Path.Combine(autoSavePath, Path.GetFileNameWithoutExtension(path) + ".xml");
                if (File.Exists(path))
                {
                    bool success = false;
                    using (var stream = File.Open(path, FileMode.OpenOrCreate))
                    {

                        try
                        {
                            result = (T)serializer.ReadObject(stream);
                            success = true;
                        }
                        catch (Exception)
                        {
                            // Assume it's an old version and try that
                            //MessageBox.Show("Wrong File type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            success = false;
                        }

                    }
                    if (!success)
                        return OldLoadNoDialog<T>(path);
                }
            }
            return result;
        }

        private static void SaveNoDialog<T>(T target, string path, string targetPath = null)
        {
            if (targetPath == null)
                targetPath = path;
            if (path != null)
            {
                var serializer = new DataContractSerializer(typeof(T));
                // Still dirty
                if (typeof(T) == typeof(Dictionary<int, TrackSelectionData>))
                {
                    try
                    {
                        Directory.CreateDirectory(autoSavePath);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            serializer.WriteObject(stream, target);
                            // Reset stream...
                            stream.Seek(0, SeekOrigin.Begin);
                            // Remove any XML data from the midi file, if any

                            // First read in the existing data
                            byte[] midiData;
                            long fsLength;
                            using (FileStream fs = File.OpenRead(path))
                            {
                                fsLength = fs.Length;
                                midiData = new byte[fs.Length];
                                for (int i = 0; i < fs.Length; i++)
                                    midiData[i] = (byte)fs.ReadByte(); // No int overflow issues here vs .Read()
                            }


                            // midiData currently includes both the xml and midi right now
                            // I need to somehow find out where in the byte array the xml stuff starts

                            //string allData = Encoding.Default.GetString(midiData);
                            //int xmlCutoff = allData.IndexOf("<TrackSelectionData") / 2;

                            int xmlCutoff = -1;
                            byte[] xmlMarker = Encoding.ASCII.GetBytes("\n<TrackSelectionData");
                            byte[] dictXmlMarker = Encoding.ASCII.GetBytes("\n<ArrayOfKeyValueOfintTrackSelectionDatatebA3aWD");

                            // We throw exceptions on these cuz we catch them below and do default saving instead if necessary
                            if (fsLength + xmlMarker.Length > int.MaxValue) // Handle rare case
                            {
                                MessageBox.Show("MIDI byte array value was larger than int maxvalue, writing to separate file instead");
                                throw new Exception("MIDI byte array value was larger than int maxvalue, writing to separate file instead");
                            }
                            if (stream.Length > int.MaxValue)
                            {
                                MessageBox.Show("Track Selection byte array value was larger than int maxvalue, writing to separate file instead");
                                throw new Exception("Track Selection byte array value was larger than int maxvalue, writing to separate file instead");
                            }

                            // I think we have to do this manually, annoyingly, there's no easy way to find the indexOf a byte array like this
                            for (int i = 0; (i + xmlMarker.Length) < midiData.Length; i++)
                            {
                                if ((i + dictXmlMarker.Length) < midiData.Length)
                                {
                                    var compare2 = midiData.Skip(i).Take(dictXmlMarker.Length).ToArray();
                                    bool success2 = true;
                                    for (int j = 0; j < dictXmlMarker.Length; j++)
                                    {
                                        if (compare2[j] != dictXmlMarker[j])
                                        {
                                            success2 = false;
                                            break;
                                        }
                                    }
                                    if (success2)
                                    {
                                        xmlCutoff = i;
                                        break;
                                    }
                                }
                                var compare = midiData.Skip(i).Take(xmlMarker.Length).ToArray();
                                // Again it doesn't seem that == works here and we have to do it by hand...
                                bool success = true;
                                for (int j = 0; j < xmlMarker.Length; j++)
                                {
                                    if (compare[j] != xmlMarker[j])
                                    {
                                        success = false;
                                        break;
                                    }
                                }
                                if (success)
                                {
                                    xmlCutoff = i;
                                    break;
                                }
                            }

                            // As a safety check - it should never be -1 or 0 or really anything low but whatever
                            if (xmlCutoff > 0)
                                midiData = midiData.Take(xmlCutoff).ToArray();

                            // Now we have all the data we need
                            using (FileStream fs = File.Create(targetPath))
                            {
                                fs.Write(midiData, 0, midiData.Length); // Midi data
                                byte[] xmlData = new byte[stream.Length];
                                stream.Read(xmlData, 0, (int)stream.Length);
                                var newline = Encoding.Default.GetBytes("\n");
                                fs.Write(newline, 0, newline.Length);
                                fs.Write(xmlData, 0, xmlData.Length);
                            }
                        }
                        return;
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show("Failed to write to midi file - writing to XML file instead");
                        var folderpath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"LuteBot","Profiles");
                        Directory.CreateDirectory(folderpath);
                        path = folderpath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + ".xml";
                        using (var stream = File.Create(path))
                        {
                            serializer.WriteObject(stream, target);
                        }
                    } // If it fails, fallback and write to the old path, and write just the stream
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
                bool success = false;
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
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        // Assume it's an old version and try that 
                        success = false;
                        MessageBox.Show("Error reading embedded data in file, trying default file");
                    }
                }
                if (!success)
                {
                    return OldLoadNoDialog<T>(path);
                }
            }
            return result;
        }

        private static T OldLoadNoDialog<T>(string path)
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
                        var serializer = new XmlSerializer(typeof(T));
                        try
                        {
                            result = (T)serializer.Deserialize(stream);
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

        private static T OldLoad<T>()
        {
            T result = default(T);
            string path = LoadFileDialogHelper();
            if (path != null)
            {
                using (var stream = File.Open(path, FileMode.OpenOrCreate))
                {
                    var serializer = new XmlSerializer(typeof(T));
                    try
                    {
                        result = (T)serializer.Deserialize(stream);
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
