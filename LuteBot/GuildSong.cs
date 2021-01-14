using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot
{
    public class GuildSong
    {
        
        public string checksum { get; set; }
        [DisplayName("Filename")]
        public string filename { get; set; }
        [DisplayName("Contributor")]
        public string contributor { get; set; }
        
        [DisplayName("Mordhau Score")]
        public int m_score { get; set; }
        public string source_url { get; set; }
    }

}
