using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.Core
{
    public abstract class Player
    {

        public event EventHandler<AsyncCompletedEventArgs> SongLoaded;
        protected bool disposed;

        public bool Disposed { get => disposed; }

        protected void LoadCompleted(object sender, AsyncCompletedEventArgs args)
        {
            SongLoaded.Invoke(sender, args);
        }

        public abstract void Stop();

        public abstract void Play();

        public abstract void Pause();

        public abstract void Dispose();

        public abstract void LoadFile(string filename);

        public abstract void ResetSoundEffects();

        public abstract void SetPosition(int position);

        public abstract int GetPosition();

        public abstract int GetLength();

        public abstract string GetFormattedPosition();

        public abstract string GetFormattedLength();
    }
}
