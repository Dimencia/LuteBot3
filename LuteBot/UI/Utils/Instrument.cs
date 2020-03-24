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
        public int LowestNote { get; set; }
        public int NoteCount { get; set; }
        public int NoteCooldown { get; set; }

        public static List<Instrument> Prefabs = new List<Instrument>();
        private static string path = $@"{Environment.CurrentDirectory}\Config\Instruments.json";

        static Instrument()
        {
            if(!File.Exists(path))
            {
                Prefabs.Add(new Instrument() { LowestNote = 0, NoteCount = 24, NoteCooldown = 18, Name = "Mordhau Lute" });
                Prefabs.Add(new Instrument() { LowestNote = 45, NoteCount = 32, NoteCooldown = 5, Name = "Rust Trumpet" });
                Prefabs.Add(new Instrument() { LowestNote = 21, NoteCount = 88, NoteCooldown = 5, Name = "Rust Piano" });
                Prefabs.Add(new Instrument() { LowestNote = 40, NoteCount = 30, NoteCooldown = 5, Name = "Rust Can Guitar" });
                Prefabs.Add(new Instrument() { LowestNote = 40, NoteCount = 27, NoteCooldown = 5, Name = "Rust Acoustic Guitar" });
                Prefabs.Add(new Instrument() { LowestNote = 72, NoteCount = 25, NoteCooldown = 5, Name = "Rust Xylobones" });
                Prefabs.Add(new Instrument() { LowestNote = 28, NoteCount = 21, NoteCooldown = 5, Name = "Rust Bass" });
                Prefabs.Add(new Instrument() { LowestNote = 36, NoteCount = 11, NoteCooldown = 5, Name = "Rust Sousaphone" });
                Prefabs.Add(new Instrument() { LowestNote = 72, NoteCount = 12, NoteCooldown = 5, Name = "Rust Flute" });
                Prefabs.Add(new Instrument() { LowestNote = 48, NoteCount = 14, NoteCooldown = 5, Name = "Rust Drums (Bad)" });
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
