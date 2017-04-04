<!--title: Http Status Codes-->

You can declaratively check the status code with this syntax:

<[sample:check-the-status-code]>

Do note that by default, if you do not specify the expected status code, Alba assumes that
the request should return 200 (OK) and will fail the scenario if a different status code is found. You
can ignore that check with the `Scenario.IgnoreStatusCode()` method.