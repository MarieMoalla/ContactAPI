using Prometheus;
using System.Diagnostics.Metrics;
using System.Threading;

namespace ContactsApp.Service
{
    public class WaetherServiceMetrics
    {
        Histogram<float> requestDuration;
        Counter FailedRequests;
        public WaetherServiceMetrics(IMeterFactory meterFactory)
        {
            var meter = meterFactory.Create("waether.service");
            requestDuration = meter.CreateHistogram<float>("RequestDuration", unit: "ms");
        }

        }
}
