# Resource Not Found Mechanics

-> id = f8a5de9e-e93f-4879-9b60-9b9d2fdb9b88
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-04T00:00:00.0000000
-> tags = 

[Router]
|> RoutesAre
    [table]
    |> RoutesAre-row HttpMethod=GET, Pattern=planets
    |> RoutesAre-row HttpMethod=GET, Pattern=planets/hoth
    |> RoutesAre-row HttpMethod=GET, Pattern=planets/naboo

|> TheResultShouldBe
    [table]
    |> TheResultShouldBe-row HttpMethod=GET, Url=/planets/tattooine, Status=404, Body=Resource not found, Arguments=NONE
    |> TheResultShouldBe-row HttpMethod=POST, Url=/planets, Status=404, Body=Resource not found, Arguments=NONE

~~~
