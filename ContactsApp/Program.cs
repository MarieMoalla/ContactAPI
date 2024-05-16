using ContactsApp.Data;
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
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;


// shared Resource to use for both OTel metrics AND tracing (shared data)
//telling otel that the exported traces all belong to Demo.AspNet
var resource = ResourceBuilder.CreateDefault().AddService("ContactsApp");

//test Trace creation called contact.source
ActivitySource tracingSource = new ActivitySource("contact.source");

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
          .AddSource("contact.source")
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddSqlClientInstrumentation()
          .AddOtlpExporter(opts =>
          {
              opts.Endpoint = new Uri("http://localhost:5341/ingest/otlp/v1/traces");
              opts.Protocol = OtlpExportProtocol.HttpProtobuf;
              opts.Headers = "X-Seq-ApiKey=abcde12345";
          })
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

//register traces service
builder.Services.AddSingleton<IContactTraces, ContactTraces>();

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

app.UseAuthorization();

app.MapControllers();

app.UseMetricServer();
//app.UseMetricMiddleware();

app.MapPrometheusScrapingEndpoint();

app.Run();
