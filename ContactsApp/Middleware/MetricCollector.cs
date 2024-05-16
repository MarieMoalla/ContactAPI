using Microsoft.CodeAnalysis;
using System.Diagnostics.Metrics;

namespace ContactsApp.Middleware
{
    public class MetricCollector
    {
        //private readonly ILogger<MetricReporter> _logger;
        //private readonly Counter<int> _requestCounter;
        //private readonly Histogram<double> _responseTimeHistogram;
        //public MetricCollector(ILogger<MetricReporter> logger)
        //{
        //    var meter = new Meter("Metric.Collector");
        //    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        //    _requestCounter = meter.CreateCounter<int>("total_requests", "The total number of requests serviced by this API.");
        //    _responseTimeHistogram = meter.CreateHistogram("request_duration_seconds", "The duration in seconds between the response to a request.", meter.CreateHistogram<double> 
        //    {
        //        Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
        //        LabelNames = new[] { "status_code", "method" }
        //    });
        //}
        //public void RegisterRequest()
        //{
        //    _requestCounter.Inc();
        //}
        //public void RegisterResponseTime(int statusCode, string method, TimeSpan elapsed)
        //{
        //    _responseTimeHistogram.Labels(statusCode.ToString(), method).Observe(elapsed.TotalSeconds);
        //}
    }
}
