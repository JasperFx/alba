<!--title: Before and after actions-->

<[info]>
The Before/After actions are **not** additive. The last one specified is the only one executed.
<[/info]>

As of Alba 2.0, you can specify actions that run immediately before or after an HTTP request is executed for common setup or teardown
work like setting up authentication credentials or tracing or whatever.

Here's a sample:

<[sample:before-and-after]>