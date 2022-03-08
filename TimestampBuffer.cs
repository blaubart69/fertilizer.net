using System;

namespace Fertilizer;

class TimestampBuffer
{
    RingBuffer<DateTime> _buf;
    public TimestampBuffer(int size)
    {
        _buf = new RingBuffer<DateTime>(size);
    }
    public void Insert(DateTime stamp)
    {
        _buf.insert(stamp);
    }
    public uint GetSignalsWithinTimespan(DateTime now, TimeSpan timeSpanToLookBack)
    {
        uint signalsWithinTimespan = 0;

        foreach (DateTime bufTimestamp in _buf.values() )
        {
            TimeSpan diff = now - bufTimestamp;
            if ( diff > timeSpanToLookBack )
            {
                break;
            }
            else
            {
                signalsWithinTimespan += 1;
            }
        }

        return signalsWithinTimespan;
    }
}