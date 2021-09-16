using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteMod.Sequencing
{
    public class Partition
    {
        private string name;
        private List<Track> tracks;
        private float tempo;

        public string Name { get => name; set => name = value; }
        public List<Track> Tracks { get => tracks; set => tracks = value; }
        public float Tempo { get => tempo; set => tempo = value; }

        public Partition()
        {
            tracks = new List<Track>();
        }

        public override string ToString()
        {
            StringBuilder strbld = new StringBuilder();

            strbld.Append("|").Append(name).Append(";").Append(tempo).Append("|");

            foreach (Track track in tracks)
            {
                strbld.Append(track.ToString()).Append("|");
            }
            strbld.Remove(strbld.Length - 1, 1);
            strbld.Append("|");

            return strbld.ToString();
        }
    }
}
