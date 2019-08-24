using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Config
{
    /// <summary>
    /// A property with a name and its associated value.
    /// </summary>
    public class Property
    {
        private PropertyItem item;
        private string value;

        /// <summary>
        /// The name of the property
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public PropertyItem Item { get => item; set => item = value; }

        /// <summary>
        /// The value of the property
        /// </summary>
        public string Value { get => value; set => this.value = value; }

        /// <summary>
        /// Generate a list of properties with their default values
        /// </summary>
        /// <returns>A complete list of properties</returns>

        public static PropertyItem FromString(string name)
        {
            PropertyItem result;
            if (Enum.TryParse(name, out result))
            {
                return result;
            }
            else
            {
                return PropertyItem.None;
            }
        }
    }
}
