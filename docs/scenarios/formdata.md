# Sending Http Form Data

Posting HTTP form data in the request can be done with the extension method shown below:

<!-- snippet: sample_write_form_data -->
<a id='snippet-sample_write_form_data'></a>
```cs
public Task write_form_data(IAlbaHost system)
{
    var form1 = new Dictionary<string, string>
    {
        ["a"] = "what?",
        ["b"] = "now?",
        ["c"] = "really?"
    };

    return system.Scenario(_ =>
    {
        // This writes the dictionary values to the HTTP
        // request as form data, and sets the content-length
        // header as well as setting the content-type
        // header to application/x-www-form-urlencoded
        _.WriteFormData(form1);
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/FormData.cs#L8-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_write_form_data' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There's a second overload that attempts to use an object and its properties to populate the form data:

<!-- snippet: sample_binding_against_a_model -->
<a id='snippet-sample_binding_against_a_model'></a>
```cs
[Fact]
public async Task can_bind_to_form_data()
{

    using (var system = AlbaHost.ForStartup<Startup>())
    {

        var input = new InputModel {
            One = "one",
            Two = "two",
            Three = "three"
        };

        await system.Scenario(_ =>
        {
            _.Post.FormData(input)
                .ToUrl("/gateway/insert");
        });

        GatewayController.LastInput.ShouldNotBeNull();

        GatewayController.LastInput.One.ShouldBe("one");
        GatewayController.LastInput.Two.ShouldBe("two");
        GatewayController.LastInput.Three.ShouldBe("three");
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/data_binding_in_mvc_app.cs#L12-L41' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_binding_against_a_model' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Do note that this only adds first level properties, so if you need to deeper accessors like add "Prop1.Prop2.Prop3,"
you'll have to resort to the dictionary approach.
