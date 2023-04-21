using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LuteBot.UI.Utils
{
    public class Instrument
    {
        public string Name { get; set; }
        public int LowestSentNote { get; set; }// These are for the idDea that lute sends a 0, when it's playing a 24... Actually a 36 but all my logic assumes 0 is C0 even though it's not...
        public int NoteCount { get; set; }
        public int NoteCooldown { get; set; }
        public int LowestPlayedNote { get; set; }
        public bool ForbidsChords { get; set; }


        public static List<Instrument> Prefabs = new List<Instrument>()
        {
            new Instrument() { LowestSentNote = 0, LowestPlayedNote = 24, NoteCount = 60, NoteCooldown = 18, Name = "Mordhau Lute" },
            new Instrument() { LowestSentNote = 0, LowestPlayedNote = 36, NoteCount = 48, NoteCooldown = 18, Name = "Mordhau Flute" },
            new Instrument() { LowestSentNote = 45, NoteCount = 32, NoteCooldown = 5, Name = "Rust Trumpet" }, // TODO: Fill out LowestPlayedNotes on Rust instruments
            new Instrument() { LowestSentNote = 21, NoteCount = 88, NoteCooldown = 5, Name = "Rust Piano" },
            new Instrument() { LowestSentNote = 40, NoteCount = 30, NoteCooldown = 5, Name = "Rust Can Guitar" },
            new Instrument() { LowestSentNote = 40, NoteCount = 27, NoteCooldown = 5, Name = "Rust Acoustic Guitar" },
            new Instrument() { LowestSentNote = 72, NoteCount = 25, NoteCooldown = 5, Name = "Rust Xylobones" },
            new Instrument() { LowestSentNote = 28, NoteCount = 21, NoteCooldown = 5, Name = "Rust Bass" },
            new Instrument() { LowestSentNote = 36, NoteCount = 11, NoteCooldown = 5, Name = "Rust Sousaphone" },
            new Instrument() { LowestSentNote = 72, NoteCount = 12, NoteCooldown = 5, Name = "Rust Flute" },
            new Instrument() { LowestSentNote = 48, NoteCount = 14, NoteCooldown = 5, Name = "Rust Drums (Bad)" }
        };
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "Config", "Instruments.json");

        static Instrument()
        {
            if (File.Exists(path))
                Read();
            else
                Write();
        }

        public static void Read()
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader stream = new StreamReader(path))
            {
                Prefabs = (List<Instrument>)serializer.Deserialize(stream, typeof(List<Instrument>));
            }
        }

        public static void Write()
        {
            JsonSerializer serializer = new JsonSerializer();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter stream = new StreamWriter(path, false))
            {
                serializer.Serialize(stream, Prefabs);
            }
        }
    }
}
