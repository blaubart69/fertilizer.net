using System;

namespace Fertilizer
{
    class DemoSignalReceiver : ISignalReceiver
    {
        public void RegisterSignal(Action<KIND_OF_SIGNAL> signal)
        {
            throw new NotImplementedException();
        }
    }
}