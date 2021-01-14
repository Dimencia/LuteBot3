using LuteBot.IO.Files;
using LuteBot.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace LuteBot.Config
{

    /// <summary>
    /// Save, load and store the configuration.
    /// </summary>
    public static class ConfigManager
    {
        private static Config configuration;
        private const string autoSavePath = @"Config\";

        static ConfigManager()
        {
            Refresh();
        }

        /// <summary>
        /// Re-Load the config from the file system
        /// </summary>
        public static void Refresh()
        {
            configuration = new Config();
            configuration = LoadConfig();
            if (configuration == null)
            {
                configuration = LoadDefaultConfig();
                configuration.Verify();
                SaveConfig();
            }
            else
            {
                configuration.Verify();
                SaveConfig();
            }
        }

        public static string GetProperty(PropertyItem item)
        {
            return configuration.Get(item);
        }

        public static void SetProperty(PropertyItem item, string value)
        {
            configuration.Set(item, value);
        }

        public static void SaveConfig()
        {
            FileIO.SaveJSON<Config>(configuration, BuildPath(configuration));
        }

        private static Config LoadConfig()
        {
            return FileIO.LoadJSON<Config>(BuildPath(configuration));
        }

        private static Config LoadDefaultConfig()
        {
            return FileIO.LoadJSONFromTxt<Config>(Resources.DefaultConfig);
        }

        public static bool GetBooleanProperty(PropertyItem item)
        {
            string temp = configuration.Get(item);
            bool result;
            if (bool.TryParse(temp, out result))
            {
                return result;
            }
            else throw new InvalidOperationException("Parsing error on property :" + item.ToString());
        }

        public static Point GetCoordsProperty(PropertyItem item)
        {
            string stringItem = configuration.Get(item);
            if (stringItem.Contains('|'))
            {
                string[] stringItemSplit = stringItem.Split('|');
                if (stringItemSplit.Length == 2)
                {
                    return new Point() { X = int.Parse(stringItemSplit[0]), Y = int.Parse(stringItemSplit[1]) };
                }
            }
            throw new InvalidOperationException("Parsing error on property :" + item.ToString());
        }

        public static Keys GetKeybindProperty(PropertyItem item)
        {
            if (Enum.TryParse<Keys>(configuration.Get(item), out Keys consoleKey))
            {
                return consoleKey;
            }
            else throw new InvalidOperationException("Parsing error on property :" + item.ToString());
        }

        public static PropertyItem GetKeybindPropertyFromAction(Keys key)
        {
            var result = configuration.Properties.FirstOrDefault(x => x.Value == key.ToString());
            if (result != null)
            {
                return result.Item;
            }
            else return PropertyItem.None;
        }

        public static int GetIntegerProperty(PropertyItem item)
        {
            string temp = configuration.Get(item);
            int result;
            if (int.TryParse(temp, out result))
            {
                return result;
            }
            else throw new InvalidOperationException("Parsing error on property :" + item.ToString());
        }

        private static string BuildPath(Config config)
        {
            return autoSavePath + "Configuration";
        }

        public static string GetVersion()
        {
            return "3.0.4";
        }
    }
}
