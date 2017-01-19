# Simple Routing Selection

-> id = 79819281-be8a-47dd-ae9b-5e063e3ebfa8
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-11-30T00:00:00.0000000
-> tags = 

[Routing]
|> RoutesAre
    [table]
    |> RoutesAre-row Route=EMPTY
    |> RoutesAre-row Route=colors
    |> RoutesAre-row Route=colors/blue
    |> RoutesAre-row Route=colors/green
    |> RoutesAre-row Route=colors/red
    |> RoutesAre-row Route=planets
    |> RoutesAre-row Route=planets/hoth
    |> RoutesAre-row Route=planets/dagobah
    |> RoutesAre-row Route=planets/naboo

|> TheSelectionShouldBe
    [table]
    |> TheSelectionShouldBe-row Url=EMPTY, Selected=EMPTY, Arguments=NONE
    |> TheSelectionShouldBe-row Url=/colors, Selected=colors, Arguments=NONE
    |> TheSelectionShouldBe-row Url=colors/blue/, Selected=colors/blue, Arguments=NONE
    |> TheSelectionShouldBe-row Url=/planets/, Selected=planets, Arguments=NONE
    |> TheSelectionShouldBe-row Url=/planets/naboo/, Selected=planets/naboo, Arguments=NONE

~~~
