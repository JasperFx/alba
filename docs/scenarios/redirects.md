# Working with Redirects

## Asserting on Expected Redirect Responses

Alba comes with some out of the box assertions to declaratively check expected redirects.

<!-- snippet: sample_asserting_redirects -->
<a id='snippet-sample_asserting_redirects'></a>
```cs
public async Task asserting_redirects(IAlbaHost system)
{
    await system.Scenario(_ =>
    {
        // should redirect to the url
        _.RedirectShouldBe("/redirect");

        // should redirect permanently to the url
        _.RedirectPermanentShouldBe("/redirect");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/Redirects.cs#L5-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_asserting_redirects' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
