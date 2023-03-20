# Tracing & Open Telemetry

Performing distributed tracing within your CI pipline is a relatively new yet powerful concept that can improve your teams response to broken, flaky or slow-performing tests. 
Alba has support for Open Telemetry tracing within `Scenario` calls, permitting tracing within your test pipeline with any compatible OpenTelemetry integration. 
If you believe there's value in tracing additional areas of Alba, please let us know!

## Automated Instrumentation

### Datadog CI Visibility

![Datadog Tracing](/tracing.png)

Datadog's CI Visibility feature is compatible with Alba, however you must be using DD .NET Tracer 2.24+ and have `DD_TRACE_OTEL_ENABLED` set to `true`. See the [documentation](https://docs.datadoghq.com/continuous_integration/tests/dotnet/) for setup information.


## Manual Instrumentation

### xUnit

Manually instrumenting your tests requires a moderate amount of supporting code to work correctly. See this [repository](https://github.com/martinjt/unittest-with-otel) and related [guide](https://www.honeycomb.io/blog/monitoring-unit-tests-opentelemetry) by the team at Honeycomb.io as a starting point.
