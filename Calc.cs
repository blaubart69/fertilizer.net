using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;

namespace Fertilizer;

class Calc
{
    const int WHEEL_PIN = 23;
    const int ROLLER_PIN = 24;
    List<Duenger> weights;
    Duenger? current;
    GpioController gpio;

    public Calc()
    {
        gpio = new GpioController();
        gpio.RegisterCallbackForPinValueChangedEvent(WHEEL_PIN,  PinEventTypes.Falling, pinChanged);
        gpio.RegisterCallbackForPinValueChangedEvent(ROLLER_PIN, PinEventTypes.Falling, pinChanged);
    }

    public void SetFertilizers(IEnumerable<Duenger> newValues)
    {
        weights = newValues.ToList();
    }    
    public void SetCurrent(string name)
    {
        current = weights.First( duenger => String.Equals(duenger.Name,name) );
    }
    void pinChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
    {
        if ( pinValueChangedEventArgs.PinNumber == WHEEL_PIN)
        {

        }
        else if ( pinValueChangedEventArgs.PinNumber == ROLLER_PIN)
        {

        }
    }
}