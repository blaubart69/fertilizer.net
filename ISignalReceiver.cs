using System;

namespace Fertilizer;

interface ISignalReceiver
{
    void RegisterSignal(Action<KIND_OF_SIGNAL> signal);
}