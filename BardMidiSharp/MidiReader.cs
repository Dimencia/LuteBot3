using BardMidiSharp.Structures.Reader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BardMidiSharp
{
    public class MidiReader
    {
        public Song LoadedSong { get; private set; }



        


        public async Task<Song> Load(string filepath)
        {
            // TODO: Consider doing the processing while we read to give it space to breathe...?
            //var bytes = await File.ReadAllBytesAsync(filepath);

            int chunkSize = int.MaxValue;

            byte[] bytes;
            using(var fs = new FileStream(filepath, FileMode.Open))
            {
                bytes = new byte[fs.Length];
                for(int i = 0; i < fs.Length; i+= chunkSize)
                    await fs.ReadAsync(bytes, i, (int)Math.Min(chunkSize,fs.Length-i));
            }


            return new SongBuilder().LoadSong(bytes);

            // The process should be that I assume they will re-use the same midi reader
            // And so each time we load, I create a new SongBuilder, and have it build the data


        }


        
    }
}
