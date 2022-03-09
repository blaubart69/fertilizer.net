using System;

namespace Fertilizer;

interface ISignalReceiver
{
    public abstract void RegisterSignal(Action<KIND_OF_SIGNAL> signal);
    
}