using System;
using Microsoft.Extensions.Logging;

namespace Fertilizer;

enum KIND_OF_SIGNAL
{
    WHEEL,
    ROLLER
}
class SignalProcessor 
{
    TimestampBuffer _bufWheel;
    TimestampBuffer _bufRoller;
    //
    // 10000 m² divediert durch 15m Breite des Düngerers
    //
    const float METERS_PER_HEKTAR = 10000f / 15.0f;
    //
    // 417 Signale für 50 Meter
    //
    const int wheel_meter = 50;
    const int wheel_signals = 417;  
    const float SIGNALS_PER_METER = (float)wheel_signals / (float)wheel_meter;

    float   _currentSignalsPerKilo = 0;
    string  _currentName = String.Empty;
    float   _currentKg = 0;
    readonly TimeSpan _timewindowForCalculations;
    DateTime? _lastRefresh = null;
    ILogger _log;
    float _overallMeters = 0;
    float _overallKilos = 0;
    float _current_kilos_per_hektar = 0;

    public SignalProcessor(ISignalReceiver signalReceiver, TimeSpan timewindowForCalculations, ILogger<SignalProcessor> logger)
    {
        _bufWheel  = new (2048);
        _bufRoller = new (2048);
        _timewindowForCalculations = timewindowForCalculations;
        signalReceiver.RegisterSignal(SignalArrived);
        _log = logger;
    }
    public float KilosPerHektar { get { return _current_kilos_per_hektar; } }
    public float OverallMeters  { get { return _overallMeters; } }
    public float OverallKilos   { get { return _overallKilos; } }
    public void SetDuenger(string name, int signals, float kg)
    {
        _currentSignalsPerKilo = (float)signals / kg;
        _currentName = name;
        _currentKg = kg;
    }
    float CalculateCurrentKiloPerHektar(DateTime now)
    {
        uint signalsWheel  = _bufWheel .GetSignalsWithinTimespan(now, _timewindowForCalculations);
        uint signalsRoller = _bufRoller.GetSignalsWithinTimespan(now, _timewindowForCalculations);
        _log.LogInformation("signals wheel/roller: {0:N4} {1:N4}", signalsWheel, signalsRoller);

        float meters_in_timespan = (float)signalsWheel  / SIGNALS_PER_METER;
        float kilos_in_timespan  = (float)signalsRoller / _currentSignalsPerKilo;

        float kilos_per_ha;
        if ( meters_in_timespan > 0 )
        {
            float kilos_per_meter = kilos_in_timespan / meters_in_timespan;
                  kilos_per_ha    = kilos_per_meter * METERS_PER_HEKTAR;
        }
        else
        {
            kilos_per_ha = 0;
        }

        return kilos_per_ha;
    }
    void AddToOverall(DateTime now)
    {
        TimeSpan diff = _lastRefresh.HasValue ? now - _lastRefresh.Value : TimeSpan.MaxValue;

        uint signalsWheel_lastDiff  = _bufWheel .GetSignalsWithinTimespan(now, diff);
        uint signalsRoller_lastDiff = _bufRoller.GetSignalsWithinTimespan(now, diff);

        float meter_since_last_refresh  = signalsWheel_lastDiff  / SIGNALS_PER_METER;
        float kilos_since_last_refresh  = signalsRoller_lastDiff / _currentSignalsPerKilo;

        _overallMeters += meter_since_last_refresh;
        _overallKilos += kilos_since_last_refresh;

        _lastRefresh = now;
    }
    public void Refresh()
    {
        DateTime now = DateTime.Now;

        _current_kilos_per_hektar = CalculateCurrentKiloPerHektar(now);
        AddToOverall(now);
        
    }
    void SignalArrived(KIND_OF_SIGNAL signal)
    {
        if ( signal == KIND_OF_SIGNAL.ROLLER)
        {
            _bufRoller.Insert(DateTime.Now);
        }
        else if ( signal == KIND_OF_SIGNAL.WHEEL)
        {
            _bufWheel.Insert(DateTime.Now);
        }
    }
}