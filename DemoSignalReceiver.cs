using System;
using System.Threading;

namespace Fertilizer
{
    class DemoSignalReceiver : ISignalReceiver
    {
        Action<KIND_OF_SIGNAL>? _onSignal;
        Timer _fakeSignalsWheel;
        Timer _fakeSignalsRoller;
        public DemoSignalReceiver()
        {
            //new System.Timers.Timer()
            _fakeSignalsWheel = new Timer(
                SendSignal,
                KIND_OF_SIGNAL.WHEEL,
                dueTime: TimeSpan.FromSeconds(1),
                period: TimeSpan.FromMilliseconds(47));
                
            _fakeSignalsRoller = new Timer(
                SendSignal,
                KIND_OF_SIGNAL.ROLLER,
                dueTime: TimeSpan.FromSeconds(1),
                period: TimeSpan.FromMilliseconds(1560));
            
        }
        void SendSignal(object? state)
        {
            if ( state != null && _onSignal != null) 
            {
                KIND_OF_SIGNAL signal = (KIND_OF_SIGNAL)state ;
                _onSignal(signal);
            }
        }
        public void RegisterSignal(Action<KIND_OF_SIGNAL> signal)
        {
            _onSignal = signal;
        }
    }
}