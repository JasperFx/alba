# Derive arguments from method signature

-> id = cf0cb7f8-8b05-4070-b0ce-83dee7839e53
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-14T00:00:00.0000000
-> tags = 

[RouteBuilder]
|> BuildRoute
    [table]
    |> BuildRoute-row HttpMethod=GET, Pattern=person/:name, Arguments=name@1:string
    ``` Method
    get_person_name(name:string)
    ```

    |> BuildRoute-row HttpMethod=GET, Pattern=query/:from/:to
    ``` Method
    get_query_from_to(from:int, to:int)
    ```

    ``` Arguments
    from@1:int, to@2:int
    ```

    |> BuildRoute-row HttpMethod=GET, Pattern=person1/:Name, Arguments=Name@1:string
    ``` Method
    get_person1_Name(input:{Name:string})
    ```

    |> BuildRoute-row HttpMethod=GET, Pattern=person2/:Name, Arguments=Name@1:string
    ``` Method
    get_person2_Name(input:{Name:string})
    ```

    |> BuildRoute-row Method=post_user_id(id:Guid), HttpMethod=POST, Pattern=user/:id, Arguments=id@1:Guid

~~~
