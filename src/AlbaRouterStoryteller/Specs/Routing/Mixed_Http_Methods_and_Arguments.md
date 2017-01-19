# Mixed Http Methods and Arguments

-> id = 7120895e-ffee-40fd-a580-4f3f4576936e
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-04T00:00:00.0000000
-> tags = 

[Router]
|> RoutesAre
    [table]
    |> RoutesAre-row HttpMethod=GET, Pattern=planets/:planet
    |> RoutesAre-row HttpMethod=POST, Pattern=planets/:planet
    |> RoutesAre-row HttpMethod=HEAD, Pattern=planets/naboo

|> TheResultShouldBe
    [table]
    |> TheResultShouldBe-row HttpMethod=GET, Url=/planets/hoth, Status=200, Body=GET: /planets/:planet, Arguments=planet: hoth
    |> TheResultShouldBe-row HttpMethod=POST, Url=/planets/naboo, Status=200, Body=POST: /planets/:planet, Arguments=planet: naboo
    |> TheResultShouldBe-row HttpMethod=HEAD, Url=/planets/naboo, Status=200, Body=HEAD: /planets/naboo, Arguments=NONE

~~~
