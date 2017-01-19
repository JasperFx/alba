# Route generation with dashes and underscores

-> id = b9fbb805-ce2a-441a-b2ea-aeea3e818702
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-14T00:00:00.0000000
-> tags = 

[RouteBuilder]
|> BuildRoute
    [table]
    |> BuildRoute-row Method=get_with__underscore(), HttpMethod=GET, Pattern=with/_underscore, Arguments=EMPTY
    |> BuildRoute-row Method=get_with_foo___bar(), HttpMethod=GET, Pattern=with/foo-bar, Arguments=EMPTY

~~~
