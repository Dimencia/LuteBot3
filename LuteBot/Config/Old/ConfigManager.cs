using LuteBot.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LuteBot.Old
{
    /**
     * Manage the configuration files of the project.
     */
    public class ConfigManager
    {

        private class Property
        {
            private string name;
            private PropertyType type;
            private string defaultValue;

            public string Name { get => name; set => name = value; }
            public PropertyType Type { get => type; set => type = value; }
            public string DefaultValue { get => defaultValue; set => defaultValue = value; }

            public Property(string name, PropertyType type, string defaultValue)
            {
                this.name = name;
                this.type = type;
                this.defaultValue = defaultValue;
            }
        }

        private enum PropertyType
        {
            Action, //program input keys
            Hotkey, //user input keys
            Property, //non keys properties
            WindowPos //specific properties with a format adapted to save coordinates
        }

        private List<ActionKey> savedConfig;

        private static readonly Property[] propertiesList = {
            new Property("SoundBoard", PropertyType.Property, "False"),
            new Property("PlayList", PropertyType.Property, "False"),
            new Property("TrackSelection", PropertyType.Property, "False"),
            new Property("OnlineSync", PropertyType.Property, "False"),
            new Property("SoundEffects", PropertyType.Property, "True"),
            new Property("AutoConsoleOpen", PropertyType.Property, "True"),
            new Property("Play", PropertyType.Hotkey, "Add"),
            new Property("Ready", PropertyType.Hotkey, "Subtract"),
            new Property("Next", PropertyType.Hotkey, "Multiply"),
            new Property("Previous", PropertyType.Hotkey, "Divide"),
            new Property("OpenConsole", PropertyType.Action, "Next"),
            new Property("OpenConsole", PropertyType.Hotkey, "Next"),
            new Property("UserSavedConsoleKey", PropertyType.Property, "Next"),
            new Property("Version", PropertyType.Property, "1.3"),
            new Property("MordhauInputIniLocation", PropertyType.Property, @"C:\Program Files (x86)\Steam\steamapps\common\Mordhau\Mordhau\Config"),
            new Property("NoteConversionMode", PropertyType.Property, "0"),
            new Property("LowestNoteId", PropertyType.Property, "0"),
            new Property("NoteCooldown", PropertyType.Property, "30"),
            new Property("AvaliableNoteCount", PropertyType.Property, "24"),
            new Property("DebugMode", PropertyType.Property, "False"),
            new Property("MainWindowPos", PropertyType.WindowPos, "0|0"),
            new Property("SoundBoardPos", PropertyType.WindowPos, "0|0"),
            new Property("PlayListPos", PropertyType.WindowPos, "0|0"),
            new Property("TrackSelectionPos", PropertyType.WindowPos, "0|0")
        };

        public ConfigManager()
        {
            savedConfig = new List<ActionKey>();
            RefreshConfigAndSave();
        }

        public void RefreshConfigAndSave()
        {
            string tempValue;
            foreach (Property property in propertiesList)
            {
                tempValue = ConfigurationManager.AppSettings.Get(property.Name);
                if (string.IsNullOrWhiteSpace(tempValue))
                {
                    tempValue = property.DefaultValue;
                    GlobalLogger.Error("ConfigManager", LoggerManager.LoggerLevel.Essential, "Could not find property :" + property.Name + ". will use default value :" + property.DefaultValue);
                }
                savedConfig.Add(new ActionKey(property.Name, tempValue));
            }
            Save();
        }

        public void RefreshConfig()
        {
            string tempValue;
            foreach (Property property in propertiesList)
            {
                tempValue = ConfigurationManager.AppSettings.Get(property.Name);
                if (string.IsNullOrWhiteSpace(tempValue))
                {
                    tempValue = property.DefaultValue;
                    GlobalLogger.Error("ConfigManager", LoggerManager.LoggerLevel.Essential, "Could not find property :" + property.Name + ". will use default value :" + property.DefaultValue);
                }
                savedConfig.Add(new ActionKey(property.Name, tempValue));
            }
        }

        public void ChangeProperty(string propertyName, string newValue)
        {
            bool found = false;
            for (int i = 0; i < savedConfig.Count; i++)
            {
                if (propertyName == savedConfig[i].Name)
                {
                    savedConfig[i].Name = propertyName;
                    savedConfig[i].Code = newValue;
                    found = true;
                }
            }
            if (!found)
            {
                GlobalLogger.Error("ConfigManager", LoggerManager.LoggerLevel.Basic, "Could not find property to change :" + propertyName + ".");
            }
        }

        public void ChangePropertyAndSave(string propertyName, string newValue)
        {
            bool found = false;
            int id = -1;
            for (int i = 0; i < savedConfig.Count; i++)
            {
                if (propertyName == savedConfig[i].Name)
                {
                    savedConfig[i].Name = propertyName;
                    savedConfig[i].Code = newValue;
                    found = true;
                    id = i;
                }
            }

            if (!found)
            {
                GlobalLogger.Error("ConfigManager", LoggerManager.LoggerLevel.Basic, "Could not find property to change :" + propertyName + ".");
            }
            else
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Remove(savedConfig[id].Name);
                config.AppSettings.Settings.Add(savedConfig[id].Name, savedConfig[id].Code);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        public void ResetProperty(string key)
        {
            string defaultValue = null;
            foreach (Property property in propertiesList)
            {
                if (property.Name == key)
                {
                    defaultValue = property.DefaultValue;
                    break;
                }
            }
            if (defaultValue != null)
            {
                savedConfig.Find(x => x.Name == key).Code = defaultValue;
            }
            else
            {
                GlobalLogger.Error("ConfigManager", LoggerManager.LoggerLevel.Basic, "Could not find property to reset :" + key + ".");
            }
        }

        public void SetWindowCoordinates(string key, Point point)
        {
            ChangeProperty(key, point.X + "|" + point.Y);
        }

        public Point GetWindowCoordinates(string key)
        {
            string rawCoords = GetWindowsPos().Find(x => x.Name == key).Code;
            string[] splitCoords = rawCoords.Split('|');
            return new Point() { X = int.Parse(splitCoords[0]), Y = int.Parse(splitCoords[1]) };
        }

        public bool GetBooleanProperty(string key)
        {
            bool.TryParse(GetProperty(key).Code, out bool result);
            return result;
        }

        public int GetIntegerProperty(string key)
        {
            int.TryParse(GetProperty(key).Code, out int result);
            return result;
        }

        public ActionKey GetProperty(string key)
        {
            return GetProperties().Find(x => x.Name == key);
        }

        public ActionKey GetAction(string key)
        {
            return GetActions().Find(x => x.Name == key);
        }
        public ActionKey GetHotkey(string keyCode)
        {
            return GetHotkeys().Find(x => x.Code == keyCode);
        }
        public List<ActionKey> GetActions()
        {
            return GetTypedProperties(PropertyType.Action);
        }

        public List<ActionKey> GetHotkeys()
        {
            return GetTypedProperties(PropertyType.Hotkey);
        }
        public List<ActionKey> GetProperties()
        {
            return GetTypedProperties(PropertyType.Property);
        }

        public List<ActionKey> GetWindowsPos()
        {
            return GetTypedProperties(PropertyType.WindowPos);
        }

        private List<ActionKey> GetTypedProperties(PropertyType type)
        {
            List<ActionKey> actionKeys = new List<ActionKey>();
            foreach (Property property in propertiesList)
            {
                if (property.Type == type)
                {
                    actionKeys.Add(new ActionKey(property.Name, ConfigurationManager.AppSettings.Get(property.Name)));
                }
            }
            return actionKeys;
        }

        public string GetVersion()
        {
            return ConfigurationManager.AppSettings["Version"];
        }

        public void Save()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            foreach (ActionKey action in savedConfig)
            {
                config.AppSettings.Settings.Remove(action.Name);
                config.AppSettings.Settings.Add(action.Name, action.Code);
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
