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
using System.Xml.Serialization;

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
            SaveNoDialog(data, fileName);
        }

        public static TrackSelectionData LoadTrackSelectionData(string fileName)
        {
            //return LoadNoDialog<TrackSelectionData>(autoSavePath + "/" + fileName);
            return LoadNoDialog<TrackSelectionData>(fileName);
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
                if (typeof(T) == typeof(TrackSelectionData))
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
                            while (line != null && !line.StartsWith("<TrackSelectionData"))
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
                    catch (Exception e) { 
                        path = "Profiles" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path); 
                    }
                }
                
                if (!path.Contains(".xml"))
                {
                    path = path + ".xml";
                }
                Directory.CreateDirectory(autoSavePath);
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

        private static void SaveNoDialog<T>(T target, string path)
        {
            if (path != null)
            {
                var serializer = new DataContractSerializer(typeof(T));
                // Still dirty
                if (typeof(T) == typeof(TrackSelectionData))
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
                                var compare = midiData.Skip(i).Take(xmlMarker.Length).ToArray();
                                // Again it doesn't seem that == works here and we have to do it by hand...
                                bool success = true;
                                for(int j = 0; j < xmlMarker.Length; j++)
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
                            if(xmlCutoff > 0)
                                midiData = midiData.Take(xmlCutoff).ToArray();

                            // Now we have all the data we need
                            using (FileStream fs = File.Create(path))
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
                    catch (Exception e) {
                        MessageBox.Show("Failed to write to midi file - writing to XML file instead");
                        path = "Profiles" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(path) + ".xml";
                        Directory.CreateDirectory(path);
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
