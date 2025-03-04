using FMR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativeRenderer
{
    public class MidiFile : IMidiLoader
    {
        public bool TickBased => false;

        public bool TimeBased => false;

        public bool CanParseOnRender => false;

        public bool Parsed => false;

        public bool PreLoaded => false;

        public int TrackCount => throw new NotImplementedException();

        public ushort Division => throw new NotImplementedException();

        public long NoteCount => throw new NotImplementedException();

        public TimeSpan Duration => throw new NotImplementedException();

        public string Name => "P/Invoke";

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public void OpenFile(string path)
        {
            throw new NotImplementedException();
        }

        public void ParseTo(double time)
        {
            throw new NotImplementedException();
        }

        public void PreLoad()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }
    }
}
