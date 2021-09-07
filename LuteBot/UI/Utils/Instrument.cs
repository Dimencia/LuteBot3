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
        public int LowestSentNote { get; set; }// These are for the idea that lute sends a 0, when it's playing a 24
        public int NoteCount { get; set; }
        public int NoteCooldown { get; set; }
        public int LowestPlayedNote { get; set; }
        

        public static List<Instrument> Prefabs = new List<Instrument>();
        private static string path = $@"{Environment.CurrentDirectory}\Config\Instruments.json";

        static Instrument()
        {
            if(!File.Exists(path))
            {
                Prefabs.Add(new Instrument() { LowestSentNote = 0, LowestPlayedNote = 24, NoteCount = 60, NoteCooldown = 10, Name = "Mordhau Lute" });
                Prefabs.Add(new Instrument() { LowestSentNote = 45, NoteCount = 32, NoteCooldown = 5, Name = "Rust Trumpet" }); // TODO: Fill out LowestPlayedNotes on Rust instruments
                Prefabs.Add(new Instrument() { LowestSentNote = 21, NoteCount = 88, NoteCooldown = 5, Name = "Rust Piano" });
                Prefabs.Add(new Instrument() { LowestSentNote = 40, NoteCount = 30, NoteCooldown = 5, Name = "Rust Can Guitar" });
                Prefabs.Add(new Instrument() { LowestSentNote = 40, NoteCount = 27, NoteCooldown = 5, Name = "Rust Acoustic Guitar" });
                Prefabs.Add(new Instrument() { LowestSentNote = 72, NoteCount = 25, NoteCooldown = 5, Name = "Rust Xylobones" });
                Prefabs.Add(new Instrument() { LowestSentNote = 28, NoteCount = 21, NoteCooldown = 5, Name = "Rust Bass" });
                Prefabs.Add(new Instrument() { LowestSentNote = 36, NoteCount = 11, NoteCooldown = 5, Name = "Rust Sousaphone" });
                Prefabs.Add(new Instrument() { LowestSentNote = 72, NoteCount = 12, NoteCooldown = 5, Name = "Rust Flute" });
                Prefabs.Add(new Instrument() { LowestSentNote = 48, NoteCount = 14, NoteCooldown = 5, Name = "Rust Drums (Bad)" });
                Write();
            }
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
            using (StreamWriter stream = new StreamWriter(path, false))
            {
                serializer.Serialize(stream, Prefabs);
            }
        }
    }
}
