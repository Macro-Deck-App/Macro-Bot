using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace MacroBot.StartupConfig;

public static class MetricsConfiguration
{
    public static void AddMetricsConfiguration(this IServiceCollection services)
    {
        services.AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddPrometheusExporter();
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MacroBot"))
                    .AddMeter("Users");
            });
    }
}