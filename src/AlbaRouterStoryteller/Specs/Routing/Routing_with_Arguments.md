# Routing with Arguments

-> id = eab1ea55-df13-488f-8e16-1490e3abb7f4
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-11-30T00:00:00.0000000
-> tags = 

[Routing]
|> RoutesAre
    [table]
    |> RoutesAre-row Route=EMPTY
    |> RoutesAre-row Route=colors
    |> RoutesAre-row Route=colors/:color
    |> RoutesAre-row Route=colors/all
    |> RoutesAre-row Route=query/:from/to/:to
    |> RoutesAre-row Route=:planet

|> TheSelectionShouldBe
    [table]
    |> TheSelectionShouldBe-row Url=EMPTY, Selected=EMPTY, Arguments=NONE
    |> TheSelectionShouldBe-row Url=colors, Selected=colors, Arguments=NONE
    |> TheSelectionShouldBe-row Url=colors/all, Selected=colors/all, Arguments=NONE
    |> TheSelectionShouldBe-row Url=colors/red, Selected=colors/:color, Arguments=color: red
    |> TheSelectionShouldBe-row Url=colors/green, Selected=colors/:color, Arguments=color: green
    |> TheSelectionShouldBe-row Url=query/1/to/5, Selected=query/:from/to/:to, Arguments=from: 1; to: 5
    |> TheSelectionShouldBe-row Url=query/5/to/10, Selected=query/:from/to/:to, Arguments=from: 5; to: 10

~~~
