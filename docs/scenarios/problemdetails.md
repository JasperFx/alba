# Problem Details

ASP.NET Core writes error responses in the [RFC 9457](https://www.rfc-editor.org/rfc/rfc9457) problem details
format from `Results.Problem()`, `Results.ValidationProblem()`, `[ApiController]` model validation and the exception
handler, and frameworks such as Wolverine do the same. Your application might too. Alba has assertions for these responses:

<!-- snippet: sample_problem_details_assertions -->
<a id='snippet-sample_problem_details_assertions'></a>
```cs
[Fact]
public async Task problem_details_members()
{
    await _host.Scenario(x =>
    {
        x.Post.Url("/withdrawals");
        x.ShouldReturnProblemDetails(409)
            .WithTitle("Insufficient funds")
            .WithDetail("The balance is 10.00")
            .WithType("https://example.com/problems/insufficient-funds")
            .WithExtension("balance", 10m)
            .WithExtension("limits", new[] { 5, 50 });
    });
}

[Fact]
public async Task validation_errors_by_field()
{
    await _host.Scenario(x =>
    {
        x.Post.Url("/deposits");
        x.ShouldReturnValidationProblem()
            .WithValidationError("Amount", "must be positive")
            .WithValidationError("amount", "two decimal places")
            .WithValidationError("Currency");
    });
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Acceptance/problem_details_assertions.cs#L44-L72' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_problem_details_assertions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

- `ShouldReturnProblemDetails(statusCode)` asserts the status code and the `application/problem+json` content type.
- `ShouldReturnValidationProblem()` also requires an `errors` member. The status code defaults to 400; pass another
  one, such as 422, if the application uses it.
- `WithTitle(...)`, `WithDetail(...)` and `WithType(...)` compare those members exactly.
- `WithExtension(name, value)` compares an extension member structurally, serializing the expected value with the
  application's JSON settings in the same way as [`JsonPath(...)`](json.md#asserting-on-json-structure).
- `WithValidationError(field)` asserts that a field has a validation error. Field names are matched without regard to
  case, because applications differ in how they case the keys. Pass a second argument to require that one of the
  field's messages contains that text.

All failures are reported together with the response body.
