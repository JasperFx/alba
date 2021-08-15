<!--title: Before and after actions-->

::: tip warning
The Before/After actions are **not** additive. The last one specified is the only one executed.
:::

As of Alba 2.0, you can specify actions that run immediately before or after an HTTP request is executed for common setup or teardown
work like setting up authentication credentials or tracing or whatever.

Here's a sample:

snippet: sample_before_and_after
