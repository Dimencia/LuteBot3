using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuteBot.Utils
{
    public class Song
    {
        [Name("Filename")]
        public string Filename { get; set; }
        private string _title = null;
        public string Title { get => string.IsNullOrWhiteSpace(_title ?? "") ? Regex.Replace(Filename, "[0-9]", "").Replace("_","").Replace(".mid","").Trim() : _title; set => _title = value; } 
        [Name("Artist/Composer")]
        public string Artist { get; set; }
        public string Tags { get; set; }
        [Name("MIDI Contributor")]
        public string MidiContributor { get; set; }
        public string Compatibility { get; set; }
        public string Upvotes { get; set; }
        [Name("Discord URL")]
        public string DiscordUrl { get; set; }


        
    }
}
