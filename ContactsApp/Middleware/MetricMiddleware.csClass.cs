using ContactsApp.Middleware;
using ContactsApp.Service;
using Microsoft.AspNetCore.Http;
using Prometheus;
using System.Diagnostics;
using System.Text;

namespace ContactsApp.MetricMiddleware
{
    public class MetricMiddleware
    {
        //represent current middlewar and invoke the next one in the pipeline
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        //new
        private readonly ContactMetrics _contactMetrics;

        public MetricMiddleware(RequestDelegate next,ILoggerFactory loggerFactory, ContactMetrics contactMetrics)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<MetricMiddleware>();

            //new
            _contactMetrics = contactMetrics;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path;
            var method = context.Request.Method;


            if (context.Request.Path == "/metrics")
            {
                var metricsData = new StringBuilder();
                metricsData.AppendLine("# HELP ContactCounter Number of created Contacts");
                metricsData.AppendLine("# TYPE ContactCounter gauge");
                metricsData.AppendLine($"ContactCounter {_contactMetrics.TotalContact()}");

                metricsData.AppendLine("# HELP ContactCreationProcessingTime Contact creation processing time in milliseconds");
                metricsData.AppendLine("# TYPE ContactCreationProcessingTime histogram");
                metricsData.AppendLine(_contactMetrics.RecordedContactCreationProcess());

                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync(metricsData.ToString());
            }
            else
            {
                await _next(context);
            }

        }
    }
    public static class MetricMiddlewareExtensions
    {
        public static IApplicationBuilder UseMetricMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MetricMiddleware>();
        }
    }
}
