# Leaf Creation and Parsing

-> id = 66819200-37b6-426a-8523-ba59fa9578ff
-> lifecycle = Regression
-> max-retries = 0
-> last-updated = 2015-12-01T00:00:00.0000000
-> tags = 

[Leaf]
|> CreateLeaf
    [table]
    |> CreateLeaf-row Route=a/b/c, NodePath=a/b, HasSpread=false, Parameters=NONE
    |> CreateLeaf-row Route=a/b/:c, NodePath=a/b, HasSpread=false, Parameters=c:2
    |> CreateLeaf-row Route=a/b/{c}, NodePath=a/b, HasSpread=false, Parameters=c:2
    |> CreateLeaf-row Route=a/:b/c/:d, NodePath=a/*/c, HasSpread=false, Parameters=b:1; d:3
    |> CreateLeaf-row Route=a/{b}/c/{d}, NodePath=a/*/c, HasSpread=false, Parameters=b:1; d:3
    |> CreateLeaf-row Route=..., NodePath=EMPTY, HasSpread=True, Parameters=spread:0
    |> CreateLeaf-row Route=a/..., NodePath=a, HasSpread=True, Parameters=spread:1
    |> CreateLeaf-row Route=a/:b/..., NodePath=a, HasSpread=True, Parameters=b:1; spread:2
    |> CreateLeaf-row Route=a/b/..., NodePath=a/b, HasSpread=True, Parameters=spread:2

~~~
