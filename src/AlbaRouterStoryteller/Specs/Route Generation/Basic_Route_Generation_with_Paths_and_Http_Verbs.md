# Basic Route Generation with Paths and Http Verbs

-> id = f26066e1-9b4d-414f-98f4-c66ecb219204
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-14T00:00:00.0000000
-> tags = 

[RouteBuilder]
|> BuildRoute
    [table]
    |> BuildRoute-row Method=get_one(), HttpMethod=GET, Pattern=one, Arguments=EMPTY
    |> BuildRoute-row Method=post_one(), HttpMethod=POST, Pattern=one, Arguments=EMPTY
    |> BuildRoute-row Method=put_one_two(), HttpMethod=PUT, Pattern=one/two, Arguments=EMPTY
    |> BuildRoute-row Method=delete_one_two(), HttpMethod=DELETE, Pattern=one/two, Arguments=EMPTY
    |> BuildRoute-row Method=head_one_two(), HttpMethod=HEAD, Pattern=one/two, Arguments=EMPTY
    |> BuildRoute-row Method=patch_one_two(), HttpMethod=PATCH, Pattern=one/two, Arguments=EMPTY
    |> BuildRoute-row Method=options_one_two(), HttpMethod=OPTIONS, Pattern=one/two, Arguments=EMPTY

~~~
