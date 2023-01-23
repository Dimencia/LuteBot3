using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LuteBot.TrackSelection
{
    public class DisplayChannelItem : MidiChannelItem, IDisposable
    {



        public int DisplayId { get; set; }

        private Color? _color = null;
        public Color color
        {
            get { if (_color == null) _color = Id < 16 ? Color.FromArgb(255 - (int)(Id % 4 * (255 / 4f)), (int)(Id % 8 * (200 / 8f)), (int)(Id % 4 * (255 / 4f))) : Color.FromArgb(SharedRandom.Next(0, 200), SharedRandom.Next(0, 200), SharedRandom.Next(0, 200)); return _color.Value; }
            private set { _color = value; }
        }

        private Brush _brush = null;
        public Brush brush { get { if (_brush == null) _brush = new SolidBrush(color); return _brush; } }

        private Pen _pen = null;
        public Pen pen { get { if (_pen == null) _pen = new Pen(brush, 4); return _pen; } }

        private Brush _alphaBrush = null;
        public Brush alphaBrush { get { if (_alphaBrush == null) _alphaBrush = new SolidBrush(Color.FromArgb(100, color.R, color.G, color.B)); return _alphaBrush; } }

        private bool disposedValue;

        public string GetDisplayName(bool showFluteRank) => showFluteRank && fluteRank > 0 ? $"{Id + 1}.{Name} (Rank {fluteRank} - {flutePercent}%)" : $"{Id + 1}.{Name}";

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _brush?.Dispose();
                    _pen?.Dispose();
                    _alphaBrush?.Dispose();
                }
                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~DisplayChannelItem()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
