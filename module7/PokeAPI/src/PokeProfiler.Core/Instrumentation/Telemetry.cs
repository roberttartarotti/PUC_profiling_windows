using System.Diagnostics;

namespace PokeProfiler.Core.Instrumentation;

public static class Telemetry
{
    public static readonly ActivitySource ActivitySource = new("PokeProfiler", "1.0.0");
}
