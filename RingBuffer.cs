using System;
using System.Collections.Generic;

class RingBuffer<T>
{
    T[] _buf;
    int _idx;
    ulong _overallValuesInserted;
    public RingBuffer(int size)
    {
        _buf = new T[size];
        _idx = -1;
        _overallValuesInserted = 0;
    }

    public void insert(T val)
    {
        _overallValuesInserted += 1;
        _idx += 1;
        if ( _idx == _buf.Length )
        {
            _idx = 0;
        }
        _buf[_idx] = val;
    }
    public IEnumerable<T> values()
    {
        int number_elements_to_yield = _overallValuesInserted < (ulong)_buf.Length ? (int)_overallValuesInserted : _buf.Length;

        int enum_idx = _idx;
        for (int i=0; i < number_elements_to_yield; ++i)
        {
            yield return _buf[enum_idx];

            enum_idx -= 1;
            if (enum_idx < 0)
            {
                enum_idx = _buf.Length - 1;
            }
        }
    }
}