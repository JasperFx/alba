using System.Text.RegularExpressions;
using Alba.Assertions;
 
namespace Alba;

public sealed class HeaderExpectations
{
    private readonly Scenario _parent;
    private readonly string _headerKey;

    public HeaderExpectations(Scenario parent, string headerKey)
    {
        _parent = parent;
        _headerKey = headerKey;
    }

    /// <summary>
    /// Asserts that there is a single header value matching 'expected'
    /// in the Http response
    /// </summary>
    /// <param name="expected"></param>
    /// <returns></returns>
    public HeaderExpectations SingleValueShouldEqual(string expected)
    {
        _parent.AssertThat(new HeaderValueAssertion(_headerKey, expected));
        return this;
    }

    /// <summary>
    /// Asserts that there is exactly one value in the response for the header
    /// </summary>
    /// <returns></returns>
    public HeaderExpectations ShouldHaveOneNonNullValue()
    {
        _parent.AssertThat(new HasSingleHeaderValueAssertion(_headerKey));
        return this;
    }

    /// <summary>
    /// Asserts that there is a single header value matching 'expected'
    /// regular expression in the Http response
    /// </summary>
    /// <returns></returns>
    public HeaderExpectations SingleValueShouldMatch(Regex regex)
    {
        _parent.AssertThat(new HeaderMatchAssertion(_headerKey, regex));
        return this;
    }

    /// <summary>
    /// Asserts that there is a single header value matching 'expected'
    /// regular expression in the Http response
    /// </summary>
    /// <returns></returns>
    public HeaderExpectations SingleValueShouldMatch(string regex)
    {
        return SingleValueShouldMatch(new Regex(regex));
    }

    /// <summary>
    /// Asserts that there are no values for this header in the Http response
    /// </summary>
    public void ShouldNotBeWritten()
    {
        _parent.AssertThat(new NoHeaderValueAssertion(_headerKey));
    }

    /// <summary>
    /// Asserts that there are the given values in the header.
    /// </summary>
    /// <param name="expectedValues"></param>
    public void ShouldHaveValues(params string[] expectedValues)
    {
        if (expectedValues.Length == 0)
            throw new ArgumentException("Expected values must contain at least one value", nameof(expectedValues));
        _parent.AssertThat(new HeaderMultiValueAssertion(_headerKey, expectedValues));
    }

    /// <summary>
    /// Asserts that there is one or more values for this header in the Http response
    /// </summary>
    public void ShouldHaveValues()
    {
        _parent.AssertThat(new HeaderExistsAssertion(_headerKey));
    }
}