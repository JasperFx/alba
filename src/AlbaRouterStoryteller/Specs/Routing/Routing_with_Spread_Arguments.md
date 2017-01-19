# Routing with Spread Arguments

-> id = 6fa8e393-de9c-43be-9771-43abfbe3efeb
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-11-30T00:00:00.0000000
-> tags = 

[Routing]
|> RoutesAre
    [table]
    |> RoutesAre-row Route=EMPTY
    |> RoutesAre-row Route=...
    |> RoutesAre-row Route=folder/...
    |> RoutesAre-row Route=planets
    |> RoutesAre-row Route=file/...
    |> RoutesAre-row Route=customer/:name/...

|> TheSelectionShouldBe
    [table]
    |> TheSelectionShouldBe-row Url=EMPTY, Selected=EMPTY, Arguments=NONE
    |> TheSelectionShouldBe-row Url=planets, Selected=planets, Arguments=NONE
    |> TheSelectionShouldBe-row Url=file, Selected=file/..., Arguments=spread: empty
    |> TheSelectionShouldBe-row Url=file/a, Selected=file/..., Arguments=spread: a
    |> TheSelectionShouldBe-row Url=file/a/b, Selected=file/...
    ``` Arguments
    spread: a, b
    ```

    |> TheSelectionShouldBe-row Url=file/a/b/c, Selected=file/...
    ``` Arguments
    spread: a, b, c
    ```

    |> TheSelectionShouldBe-row Url=folder/a/b/c, Selected=folder/...
    ``` Arguments
    spread: a, b, c
    ```

    |> TheSelectionShouldBe-row Url=customer/BigCo, Selected=customer/:name/...
    ``` Arguments
    name: BigCo; spread: empty
    ```

    |> TheSelectionShouldBe-row Url=customer/BigCo/1/2/3, Selected=customer/:name/...
    ``` Arguments
    name: BigCo; spread: 1, 2, 3
    ```


~~~
