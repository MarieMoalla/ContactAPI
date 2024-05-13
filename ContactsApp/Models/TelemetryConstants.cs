using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace ContactsApp.Models;

public static class TelemetryConstants
{
    /// <summary>
    /// The name of the ActivitySource/Tracer that is going to produce our TRACES and
    /// the Meter that is going to produce our METRICS.
    /// </summary>
    public const string MyAppSource = "ContactsApp";

    public static readonly ActivitySource DemoTracer = new ActivitySource(MyAppSource);

    public static readonly Meter DemoMeter = new Meter(MyAppSource);

    public static readonly Counter<long> HitsCounter =
        DemoMeter.CreateCounter<long>("IndexHits", "hits", "number of hits to homepage");
}
