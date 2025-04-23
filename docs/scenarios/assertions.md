# Writing Custom Assertions

::: tip
All of the assertions are applied during the Scenario execution, and any
failures will be reported out in the Exception message thrown by Alba on Scenario failures.
:::

The Scenario assertions in Alba are completely extensible and you can happily add your own via extension methods - but
please send anything that's generally useful as a pull request to Alba itself ;-)

The first step is to write your own implementation of this interface:

<!-- snippet: sample_IScenarioAssertion -->
<a id='snippet-sample_iscenarioassertion'></a>
```cs
public interface IScenarioAssertion
{
    void Assert(Scenario scenario, AssertionContext context);
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/IScenarioAssertion.cs#L3-L8' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_iscenarioassertion' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

As an example, here's the assertion from Alba that validates that the response body is supposed to 

<!-- snippet: sample_BodyContainsAssertion -->
<a id='snippet-sample_bodycontainsassertion'></a>
```cs
internal sealed class BodyContainsAssertion : IScenarioAssertion
{
    public string Text { get; set; }

    public BodyContainsAssertion(string text)
    {
        Text = text;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        // Context has this useful extension to read the body as a string.
        // This will bake the body contents into the exception message to make debugging easier.
        var body = context.ReadBodyAsString();
        if (!body.Contains(Text))
        {
            // Add the failure message to the exception. This exception only
            // gets thrown if there are failures.
            context.AddFailure($"Expected text '{Text}' was not found in the response body");
        }
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/Assertions/BodyContainsAssertion.cs#L3-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_bodycontainsassertion' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Once you have your assertion class, you can apply it to a scenario through an extension method against the 
`Scenario` class. Here's the `Scenario.ContentShouldContain(text)` implementation from Alba itself:

<!-- snippet: sample_ContentShouldContain -->
<a id='snippet-sample_contentshouldcontain'></a>
```cs
/// <summary>
/// Assert that the Http response contains the designated text
/// </summary>
/// <param name="scenario"></param>
/// <param name="text"></param>
/// <returns></returns>
public static Scenario ContentShouldContain(this Scenario scenario, string text)
{
    return scenario.AssertThat(new BodyContainsAssertion(text));
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba/ScenarioExpectationsExtensions.cs#L8-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_contentshouldcontain' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Finally, use your new assertion in a Scenario like this:

<!-- snippet: sample_using_ContentShouldBe -->
<a id='snippet-sample_using_contentshouldbe'></a>
```cs
[Fact]
public Task using_scenario_with_ContentShouldContain_declaration_happy_path()
{
    router.Handlers["/one"] = c =>
    {
        c.Response.Write("**just the marker**");
        return Task.CompletedTask;
    };

    return host.Scenario(x =>
    {
        x.Get.Url("/one");
        x.ContentShouldContain("just the marker");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/asserting_against_the_response_body_text.cs#L7-L23' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_using_contentshouldbe' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
