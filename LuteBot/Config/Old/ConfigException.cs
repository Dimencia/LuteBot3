using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot
{
    public class ConfigException : Exception
    {
        public ConfigException(String message) : base(message)
        {
        }
    }
}
