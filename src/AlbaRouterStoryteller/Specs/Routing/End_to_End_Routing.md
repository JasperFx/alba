# Router can differentiate between methods

-> id = 2d2a08cd-b0ff-42ef-a004-ae94b337f2dc
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-04T00:00:00.0000000
-> tags = 

[Router]
|> RoutesAre
    [table]
    |> RoutesAre-row HttpMethod=GET, Pattern=EMPTY
    |> RoutesAre-row HttpMethod=POST, Pattern=EMPTY
    |> RoutesAre-row HttpMethod=DELETE, Pattern=EMPTY
    |> RoutesAre-row HttpMethod=PUT, Pattern=EMPTY
    |> RoutesAre-row HttpMethod=HEAD, Pattern=EMPTY

|> TheResultShouldBe
    [table]
    |> TheResultShouldBe-row HttpMethod=GET, Url=EMPTY, Status=200, Body=GET: /, Arguments=NONE
    |> TheResultShouldBe-row HttpMethod=POST, Url=EMPTY, Status=200, Body=POST: /, Arguments=NONE
    |> TheResultShouldBe-row HttpMethod=DELETE, Url=EMPTY, Status=200, Body=DELETE: /, Arguments=NONE
    |> TheResultShouldBe-row HttpMethod=PUT, Url=EMPTY, Status=200, Body=PUT: /, Arguments=NONE
    |> TheResultShouldBe-row HttpMethod=HEAD, Url=EMPTY, Status=200, Body=HEAD: /, Arguments=NONE

~~~
