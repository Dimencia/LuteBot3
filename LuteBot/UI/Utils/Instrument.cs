using Newtonsoft.Json;

namespace LuteBot.UI.Utils
{
    public class Instrument
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int LowestSentNote { get; set; }// These are for the idDea that lute sends a 0, when it's playing a 24... Actually a 36 but all my logic assumes 0 is C0 even though it's not...
        public int NoteCount { get; set; }
        public int NoteCooldown { get; set; }
        public int LowestPlayedNote { get; set; }

        public float AIMinimum { get; set; } = 0f; // The minimum AI rating (0-1) that should be enabled for this instrument (inclusive)
        public float AIMaximum { get; set; } = 1f; // The maximum AI rating (0-1) that should be enabled for this instrument (inclusive)

        public static Dictionary<int, Instrument> Prefabs = new Dictionary<int, Instrument>();
        public static Dictionary<int, Instrument> DefaultPrefabs = new Dictionary<int, Instrument>()
        {
            {0, new Instrument() { Id = 0, LowestSentNote = 0, LowestPlayedNote = 24, NoteCount = 60, NoteCooldown = 18, Name = "Mordhau Lute", AIMinimum = 0f, AIMaximum = 0.60f } },
            {1, new Instrument() { Id = 1, LowestSentNote = 0, LowestPlayedNote = 36, NoteCount = 48, NoteCooldown = 18, Name = "Mordhau Flute", AIMinimum = 0.50f, AIMaximum = 1f } }
        };
        private static string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LuteBot", "Config", "Instruments.json");

        static Instrument()
        {
            Prefabs = DefaultPrefabs;
            try
            {
                if (File.Exists(path))
                    try
                    {
                        Read();
                    }
                    catch
                    {
                        Write();
                    }
                else
                    Write();
            }
            catch { } // If we can't access the files, it's fine, everything works anyway
        }

        public static void Read()
        {
            JsonSerializer serializer = new JsonSerializer();
            using (StreamReader stream = new StreamReader(path))
            {
                Prefabs = (Dictionary<int, Instrument>)serializer.Deserialize(stream, typeof(Dictionary<int, Instrument>));
            }
        }

        public static void Write(bool defaults = false)
        {
            JsonSerializer serializer = new JsonSerializer();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (StreamWriter stream = new StreamWriter(path, false))
            {
                if (defaults)
                {
                    serializer.Serialize(stream, DefaultPrefabs);
                    Prefabs = DefaultPrefabs;
                }
                else
                    serializer.Serialize(stream, Prefabs);
            }
        }
    }
}

