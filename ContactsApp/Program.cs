using ContactsApp.Data;
using ContactsApp.Models;
using ContactsApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Globalization;


// shared Resource to use for both OTel metrics AND tracing (shared data)
//telling otel that the exported traces all belong to Demo.AspNet
var resource = ResourceBuilder.CreateDefault().AddService("ContactsApp");

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
          .AddOtlpExporter(opts => opts.Endpoint = new Uri("http://localhost:4317"))
          .AddConsoleExporter())
      .WithMetrics(metrics => metrics
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation().AddPrometheusExporter()
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

app.UseAuthorization();

app.MapControllers();

app.Run();
