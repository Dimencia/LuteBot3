using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Config
{
    /// <summary>
    /// The class used by the ConfigManager to store the configuration.
    /// </summary>
    public class Config
    {
        private List<Property> properties;

        public Config()
        {
            properties = new List<Property>();
        }

        public List<Property> Properties { get => properties; set => properties = value; }

        public string Get(PropertyItem item)
        {
            return properties.Find(x => x.Item == item).Value;
        }

        public void Set(PropertyItem item, string value)
        {
            properties.Find(x => x.Item == item).Value = value;
        }

        public List<Property> GetAllProperties()
        {
            return properties;
        }

        public void Verify()
        {
            if (Enum.GetValues(typeof(PropertyItem)).Length != properties.Count)
            {
                Property property;
                foreach (PropertyItem item in Enum.GetValues(typeof(PropertyItem)))
                {
                    property = properties.FirstOrDefault(x => x.Item == item);
                    if (property == null)
                    {
                        properties.Add(new Property() { Item = item, Value = "" });
                    }
                }
            }
        }
    }
}
