using System;
using System.Device.Gpio;

namespace Fertilizer;

class GpioSignalReceiver : ISignalReceiver
{
    const int WHEEL_PIN = 23;
    const int ROLLER_PIN = 24;
    GpioController _gpio;
    Action<KIND_OF_SIGNAL>? _onSignal;
    public GpioSignalReceiver()
    {
        _gpio = new GpioController();
        _gpio.OpenPin(WHEEL_PIN, PinMode.Input);
        _gpio.OpenPin(ROLLER_PIN, PinMode.Input);
        _gpio.RegisterCallbackForPinValueChangedEvent(WHEEL_PIN,  PinEventTypes.Falling, pinChanged);
        _gpio.RegisterCallbackForPinValueChangedEvent(ROLLER_PIN, PinEventTypes.Falling, pinChanged);
    }

    public void RegisterSignal(Action<KIND_OF_SIGNAL> signal)
    {
        _onSignal = signal;
    }

    void pinChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        if (_onSignal != null)
        {
            if ( pinValueChangedEventArgs.PinNumber == WHEEL_PIN)
            {
                _onSignal(KIND_OF_SIGNAL.WHEEL);
            }
            else if ( pinValueChangedEventArgs.PinNumber == ROLLER_PIN)
            {
                _onSignal(KIND_OF_SIGNAL.ROLLER);
            }
        }
    }
}