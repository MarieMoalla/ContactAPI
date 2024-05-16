using ContactsApp.Data;
using ContactsApp.MetricMiddleware;
using ContactsApp.Models;
using ContactsApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Prometheus;
using System.Diagnostics.Metrics;
using System.Globalization;


// shared Resource to use for both OTel metrics AND tracing (shared data)
//telling otel that the exported traces all belong to Demo.AspNet
var resource = ResourceBuilder.CreateDefault().AddService("ContactsApp");

// Custom Metrics to count requests for each endpoint and the method
var counter = Metrics.CreateCounter("peopleapi_path_counter", "Counts requests to the People API endpoints", new CounterConfiguration
{
    LabelNames = new[] { "method", "endpoint" }
});

// Add services to the container.
var builder = WebApplication.CreateBuilder(args);

//inject service for logging
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(
            ResourceBuilder.CreateDefault()
                .AddService(TelemetryConstants.MyAppSource))
        .AddConsoleExporter();
});

using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource(TelemetryConstants.MyAppSource)
    .SetResourceBuilder(
        ResourceBuilder.CreateDefault()
            .AddService(serviceName: TelemetryConstants.MyAppSource))
    .AddConsoleExporter()
    .Build();

//inject service for tracing and metrcs
builder.Services.AddOpenTelemetry()
      .ConfigureResource(resource => resource.AddService(TelemetryConstants.MyAppSource))
      .WithTracing(tracing => tracing
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddSqlClientInstrumentation()
          .AddOtlpExporter(opts =>
          {
              opts.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
              opts.Protocol = OtlpExportProtocol.HttpProtobuf;
              opts.Headers = "X-Seq-ApiKey=abcde12345";
          }
            )
          .AddConsoleExporter())
      .WithMetrics(metrics => metrics
          //
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddPrometheusExporter()
          .AddMeter("contact.service")
          //.AddPrometheusHttpListener(opt => opt.UriPrefixes = new string[] { "https://localhost:7193" })
          //.AddOtlpExporter(opts => opts.Endpoint = new Uri("http://localhost:9090"))
          .AddConsoleExporter());

builder.Services.AddSingleton(TracerProvider.Default.GetTracer(TelemetryConstants.MyAppSource));

//Register metric service with DI
builder.Services.AddSingleton<ContactMetrics>();

builder.Services.AddControllers();
builder.Services.AddDbContext<ContactsAppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"));
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();



app.UseMetricServer();
app.UseHttpMetrics();

app.UseAuthorization();

app.MapControllers();

//app.UseMetricServer();

//app.UseMetricMiddleware();
app.Map("/metrics", innerApp =>
{
    innerApp.UseMetricMiddleware();
    innerApp.UseMetricServer();
});

app.MapPrometheusScrapingEndpoint();

app.Run();
