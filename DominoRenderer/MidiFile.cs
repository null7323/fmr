using FMR.Core;
using FMR.Core.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8625
namespace DominoRenderer
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Note : IUnmanagedComparable<Note>
    {
        public byte Key;
        public byte Velocity;

        public ushort Track;
        public uint Start;
        public uint End;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsBlackKey(int key)
        {
            return (key % 12) switch
            {
                0 or 2 or 4 or 5 or 7 or 9 or 11 => false,
                _ => true,
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(in Note left, in Note right)
        {
            return left.Start < right.Start || (left.Track < right.Track && left.Start == right.Start);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Tempo
    {
        public uint Tick;
        public uint Value;
        public double ActualTime;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TimeSignature(uint tick, int numerator = 4, int denominator = 4)
    {
        public uint Tick = tick;
        public int Numerator = numerator;
        public int Denominator = denominator;
    }

    internal unsafe struct MidiStream(string path) : IDisposable
    {
        private readonly FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        private bool disposed = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ushort ReadInt16()
        {
            int hi = stream.ReadByte();
            return (ushort)((hi << 8) | stream.ReadByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly uint ReadInt32()
        {
            int h1, h2, h3;
            h1 = stream.ReadByte();
            h2 = stream.ReadByte();
            h3 = stream.ReadByte();
            return (uint)((h1 << 24) | (h2 << 16) | (h3 << 8) | stream.ReadByte());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly byte ReadInt8()
        {
            return (byte)stream.ReadByte();
        }

        public readonly long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ulong Read(void* contentBuffer, ulong size, ulong count)
        {
            byte* buffer = (byte*)contentBuffer;
            ulong readSize = size * count;
            Span<byte> bufferSpan;
            ulong receivedBytes = 0;
            while (readSize > int.MaxValue)
            {
                bufferSpan = new(buffer, int.MaxValue);
                receivedBytes += (uint)stream.Read(bufferSpan);
                buffer += int.MaxValue;
                readSize -= int.MaxValue;
            }
            bufferSpan = new(buffer, (int)readSize);
            receivedBytes += (uint)stream.Read(bufferSpan);
            return receivedBytes;
        }

        public readonly bool Associated
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => !disposed;
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            stream.Dispose();
            GC.SuppressFinalize(this);
            disposed = true;
        }
    }
    internal unsafe struct RenderTrackInfo
    {
        public long Position;
        public long Size;
        public uint TrackTime;
        public UnmanagedList<Note> Notes;
    }
    public unsafe class MidiFile : IMidiLoader, IDisposable
    {
        /// <summary>
        /// 音轨数.
        /// </summary>
        protected ushort trackCount;
        /// <summary>
        /// 分解度.
        /// </summary>
        protected ushort division;
        /// <summary>
        /// Midi时长，单位为tick.
        /// </summary>
        protected uint midiTime = 0;
        /// <summary>
        /// 总音符数.
        /// </summary>
        protected long noteCount = 0;
        protected bool parsed = false;
        protected double durationInMicroseconds = 0;
        protected string midiPath;
        public UnmanagedList<Note>[] Notes = new UnmanagedList<Note>[128];
        public UnmanagedList<Tempo> Tempos = new();
        public UnmanagedList<TimeSignature> TimeSignatures = new();
        /// <inheritdoc/>
        public void PreLoad()
        {
            if (parsed) return;

            MidiStream stream = new(MidiPath);
            sbyte* hdr = stackalloc sbyte[4];

            _ = stream.Read(hdr, 4, 1);
            // 判断是否与MThd相等
            if (!(hdr[0] == 'M' && hdr[1] == 'T' && hdr[2] == 'h' && hdr[3] == 'd'))
            {
                ThrowHelper.Throw("Midi file corrupted: should receive string literal \"MThd\" at first.");
            }
            uint hdrSize = stream.ReadInt32();
            if (hdrSize != 6)
            {
                ThrowHelper.Throw("Midi file corrupted: head size should be 6.");
            }
            if (stream.ReadInt16() == 2)
            {
                ThrowHelper.ThrowNotSupported("Midi file not supported.");
            }
            trackCount = stream.ReadInt16();
            division = stream.ReadInt16();

            Logging.Write(LogLevel.Info, $"Track count: {trackCount}, division: {division}.");

            if (division > 32767)
            {
                Logging.Write(LogLevel.Error, "Unsupported division.");
                ThrowHelper.ThrowNotSupported("Division should be less than 32768.");
            }

            RenderTrackInfo[] trkInfo = new RenderTrackInfo[trackCount];
            for (int i = 0; i != trackCount; ++i)
            {
                _ = stream.Read(hdr, 4, 1);
                if (!(hdr[0] == 'M' && hdr[1] == 'T' && hdr[2] == 'r' && hdr[3] == 'k'))
                {
                    ThrowHelper.Throw("Midi file corrupted: should receive string literal \"MTrk\" at track head.");
                }
                trkInfo[i] = new RenderTrackInfo
                {
                    Size = stream.ReadInt32(),
                    Notes = new UnmanagedList<Note>(),
                    TrackTime = 0,
                    Position = stream.Position
                };
                stream.Position += trkInfo[i].Size;
                Logging.Write(LogLevel.Info, $"Track {i}, size {trkInfo[i].Size}.");
            }
            for (int i = 0; i != 128; ++i)
            {
                Notes[i] = new UnmanagedList<Note>();
            }
            Lock ioLock = new();

            _ = Parallel.For(0, trackCount, (i) =>
            {
                long trackSize = trkInfo[i].Size;
                byte* data = UnsafeMemory.Allocate<byte>(trackSize);
                lock (ioLock)
                {
                    stream.Position = trkInfo[i].Position;
                    stream.Read(data, (ulong)trackSize, 1);
                }
                UnmanagedList<Note> nl = trkInfo[i].Notes; // pass by ref
                ForwardLinkedList<long>[] fll = new ForwardLinkedList<long>[128];
                for (int j = 0; j != 128; ++j)
                {
                    fll[j] = [];
                }

                uint trkTime = 0;
                bool loop = true;
                byte* p = data;
                byte prev = 0;
                byte comm;
                while (loop)
                {
                    trkTime += ParseVLInt(ref p);
                    comm = *p++;
                    if (comm < 0x80)
                    {
                        comm = prev;
                        --p;
                    }
                    prev = comm;
                    switch (comm & 0b11110000)
                    {
                        case 0x80:
                            {
                                byte k = *p++;
                                ++p;
                                if (fll[k].Any())
                                {
                                    long idx = fll[k].Pop();
                                    nl[idx].End = trkTime;
                                }
                            }
                            continue;
                        case 0x90:
                            {
                                byte k = *p++;
                                byte v = *p++;
                                if (v != 0)
                                {
                                    long idx = nl.Count;
                                    fll[k].Add(idx);
                                    nl.Add(new Note
                                    {
                                        Start = trkTime,
                                        Key = k,
                                        Velocity = v,
                                        Track = (ushort)i
                                    });
                                }
                                else
                                {
                                    if (fll[k].Any())
                                    {
                                        long idx = fll[k].Pop();
                                        nl[idx].End = trkTime;
                                    }
                                }
                            }
                            continue;
                        case 0xA0:
                        case 0xB0:
                        case 0xE0:
                            p += 2;
                            continue;
                        case 0xC0:
                        case 0xD0:
                            ++p;
                            continue;
                        default:
                            break;
                    }
                    switch (comm)
                    {
                        case 0xF0:
                            while (*p++ != 0xF7)
                            {

                            }
                            continue;
                        case 0xF1:
                            continue;
                        case 0xF2:
                        case 0xF3:
                            p += 0xF4 - comm;
                            continue;
                        default:
                            break;
                    }
                    if (comm < 0xFF)
                    {
                        continue;
                    }
                    comm = *p++;
                    if (comm >= 0 && comm <= 0x0A)
                    {
                        uint l = ParseVLInt(ref p); // 这个中间变量不可以去掉
                        p += l;
                        continue;
                    }
                    switch (comm)
                    {
                        case 0x2F:
                            ++p;
                            for (int k = 0; k != 128; ++k)
                            {
                                foreach (long l in fll[k])
                                {
                                    nl[l].End = trkTime;
                                }
                            }
                            loop = false;
                            break;
                        case 0x51:
                            _ = ParseVLInt(ref p);
                            byte b1 = *p++;
                            byte b2 = *p++;
                            uint t = (uint)((b1 << 16) | (b2 << 8) | (*p++));
                            lock (Tempos)
                            {
                                Tempos.Add(new Tempo
                                {
                                    Tick = trkTime,
                                    Value = t
                                });
                            }
                            break;
                        case 0x58:
                            _ = ParseVLInt(ref p);
                            int numerator = *p++;
                            int denominator = *p++;
                            lock (TimeSignatures)
                            {
                                TimeSignatures.Add(new TimeSignature(trkTime, numerator, (int)Math.Pow(2, denominator)));
                            }
                            p += 2;
                            break;
                        default:
                            uint dl = ParseVLInt(ref p); // 这个中间变量不可以去掉
                            p += dl;
                            break;
                    }
                }
                UnsafeMemory.Free(data);
                trkInfo[i].TrackTime = trkTime;

                Logging.Write(LogLevel.Info, $"Track {i} parsed.");
            });
            stream.Dispose();
            Logging.Write(LogLevel.Info, "Merging events.");

            #region Note Dispatch

            for (int i = 0; i != trackCount; ++i)
            {
                if (trkInfo[i].TrackTime > midiTime)
                {
                    midiTime = trkInfo[i].TrackTime;
                }
                noteCount += trkInfo[i].Notes.Count;
            }
            for (int i = 0; i != trackCount; ++i)
            {
                if (trkInfo[i].Notes.Count == 0)
                {
                    continue;
                }
                for (long j = 0, len = trkInfo[i].Notes.Count; j != len; ++j)
                {
                    ref Note n = ref trkInfo[i].Notes[j];
                    Notes[n.Key].Add(n);
                }
                trkInfo[i].Notes.Clear();
            }
            _ = Parallel.ForEach(Notes, (nl) =>
            {
                nl.TrimExcess();
                nl.Sort(&NotePredicate);
            });
            #endregion

            #region Tempo
            if (Tempos.Count == 0)
            {
                Tempos.Add(new Tempo
                {
                    Tick = 0,
                    Value = 500000
                });
            }

            Tempos.TrimExcess();

            {
                Tempo[] tempos = Tempos.ToArray();
                SortHelper<Tempo>.StableSort(tempos, (t) => t.Tick, Tempos.First());
            }

            if (Tempos[0].Tick > 0)
            {
                Tempos.Insert(0, new Tempo
                {
                    Tick = 0,
                    Value = 500000
                });
            }
            #endregion

            #region Time Signature
            if (TimeSignatures.Count == 0)
            {
                TimeSignatures.Add(new TimeSignature(0));
            }
            TimeSignatures.Sort(&TimeSignaturePredicate);
            if (TimeSignatures[0].Tick > 0)
            {
                TimeSignatures.Insert(0, new TimeSignature(0));
            }
            #endregion

            AssignActualTime();
            parsed = true;
        }
        /// <summary>
        /// 初始化一个<see cref="MidiFile"/>对象.
        /// </summary>
        public MidiFile()
        {
            midiPath = string.Empty;
        }

        /// <inheritdoc/>
        public void OpenFile(string path)
        {
            midiPath = path;
            if (!File.Exists(path))
            {
                ThrowHelper.ThrowFileNotExist(path);
            }
        }

        /// <summary>
        /// 当前Midi的路径.
        /// </summary>
        public string MidiPath => midiPath;

        /// <summary>
        /// 获取变长整数.
        /// </summary>
        /// <param name="p">数据源. 指向数据的指针会在调用本方法后改变.</param>
        /// <returns>变长整数值.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static unsafe uint ParseVLInt(ref byte* p)
        {
            uint result = 0;
            uint b;
            do
            {
                b = *p++;
                result = (result << 7) | (b & 0x7F);
            }
            while ((b & 0b10000000) != 0);
            return result;
        }

        /// <summary>
        /// 比较<see cref="Tempo"/>的先后.
        /// </summary>
        /// <param name="left">第一个<see cref="Tempo"/>事件.</param>
        /// <param name="right">第二个<see cref="Tempo"/>事件.</param>
        /// <returns>如果第一个<see cref="Tempo"/>事件的<see cref="Tempo.Tick"/>更小，返回<see langword="true"/>，否则返回<see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool TempoPredicate(in Tempo left, in Tempo right) 
        {
            return left.Tick < right.Tick;
        }

        /// <summary>
        /// 比较<see cref="TimeSignature"/>的先后.
        /// </summary>
        /// <param name="left">第一个<see cref="TimeSignature"/>事件.</param>
        /// <param name="right">第二个<see cref="TimeSignature"/>事件.</param>
        /// <returns><paramref name="left"/>是否在<paramref name="right"/>前.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool TimeSignaturePredicate(in TimeSignature left, in TimeSignature right)
        {
            return left.Tick < right.Tick;
        }

        /// <summary>
        /// 比较<see cref="Note"/>的先后.
        /// </summary>
        /// <param name="left">第一个<see cref="Note"/>事件.</param>
        /// <param name="right">第二个<see cref="Note"/>事件.</param>
        /// <returns>如果第一个<see cref="Note"/>事件的绘制顺序优先，返回<see langword="true"/>，否则返回<see langword="false"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool NotePredicate(in Note left, in Note right) 
        {
            return left.Start < right.Start || (left.Track < right.Track && left.Start == right.Start);
        }

        /// <summary>
        /// 将tempo的tick时间转化为具体时间，单位微秒.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void AssignActualTime()
        {
            uint lastTempoTick = 0;
            double lastTempoTime = 0.0;
            int ppq = division;
            double microsecondsPerTick = 500000.0 / ppq;
            for (long i = 0; i < Tempos.Count; i++)
            {
                ref Tempo tempo = ref Tempos[i];
                uint diff = tempo.Tick - lastTempoTick;
                
                tempo.ActualTime = lastTempoTime + microsecondsPerTick * diff;

                lastTempoTick = tempo.Tick;
                lastTempoTime = tempo.ActualTime;
                microsecondsPerTick = (double)tempo.Value / ppq;
            }
            uint diffTickTillEnd = midiTime - lastTempoTick;
            durationInMicroseconds = lastTempoTime + microsecondsPerTick * diffTickTillEnd;
        }

        /// <inheritdoc/>
        public void ParseTo(double time)
        {
            return;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (Notes != null)
            {
                foreach (var notes in Notes)
                {
                    notes.Dispose();
                }
            }
            Notes = null;
            Tempos?.Dispose();
            Tempos = null;
            TimeSignatures?.Dispose();
            TimeSignatures = null;
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public void Reset()
        {
            if (Notes != null)
            {
                foreach (var notes in Notes)
                {
                    notes.Dispose();
                }
            }
            Tempos?.Clear();
            TimeSignatures?.Clear();
            Notes = new UnmanagedList<Note>[128];
            trackCount = 0;
            division = 0;
            midiTime = 0;
            noteCount = 0;
            parsed = false;
            durationInMicroseconds = 0.0;
            midiPath = string.Empty;
    }

        /// <inheritdoc/>
        public bool TickBased => true;
        /// <inheritdoc/>
        public bool TimeBased => false;
        /// <inheritdoc/>
        public bool Parsed => parsed;
        /// <inheritdoc/>
        public bool PreLoaded => parsed;
        /// <inheritdoc/>
        public bool CanParseOnRender => false;
        /// <inheritdoc/>
        public int TrackCount => trackCount;
        /// <inheritdoc/>
        public ushort Division => division;
        /// <inheritdoc/>
        public long NoteCount => noteCount;
        /// <inheritdoc/>
        public TimeSpan Duration => new ((long)(durationInMicroseconds * 10.0));
        /// <summary>
        /// 表示Midi时长，单位tick.
        /// </summary>
        public uint MidiTime => midiTime;

        public string Name => "Domino Midi Loader";
    }
}
#pragma warning restore CS8625
