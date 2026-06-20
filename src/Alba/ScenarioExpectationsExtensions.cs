using System.Net;
using Alba.Assertions;

namespace Alba;

public static class ScenarioExpectationsExtensions
{
    #region sample_ContentShouldContain

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

    #endregion

    /// <summary>
    /// Assert that the Http response does not contain the designated text
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static Scenario ContentShouldNotContain(this Scenario scenario, string text)
    {
        return scenario.AssertThat(new BodyDoesNotContainAssertion(text));
    }

    /// <summary>
    /// Assert that the Http response body text is exactly the expected content
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="exactContent"></param>
    /// <returns></returns>
    public static Scenario ContentShouldBe(this Scenario scenario, string exactContent)
    {
        return scenario.AssertThat(new BodyTextAssertion(exactContent));
    }

    /// <summary>
    /// Assert that the Http Status Code is 200 (Ok)
    /// </summary>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static Scenario StatusCodeShouldBeOk(this Scenario scenario)
    {
        return scenario.StatusCodeShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// Assert that the Http Status Code is between 200 and 299
    /// </summary>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static Scenario StatusCodeShouldBeSuccess(this Scenario scenario)
    {   
        //scenario.IgnoreStatusCode();
        return scenario.AssertThat(new StatusCodeSuccessAssertion());
    }

    /// <summary>
    /// Assert that the content-type header value of the Http response
    /// matches the expected value
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="mimeType"></param>
    /// <returns></returns>
    public static Scenario ContentTypeShouldBe(this Scenario scenario, MimeType mimeType)
    {
        return scenario.ContentTypeShouldBe(mimeType.Value);
    }


    /// <summary>
    /// Assert that the content-type header value of the Http response
    /// matches the expected value
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="mimeType"></param>
    /// <returns></returns>
    public static Scenario ContentTypeShouldBe(this Scenario scenario, string mimeType)
    {
        scenario.Header("content-type").SingleValueShouldEqual(mimeType);
        return scenario;
    }

    /// <summary>
    /// Assert that the http response was redirected
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="expected"></param>
    /// <returns></returns>
    public static Scenario RedirectShouldBe(this Scenario scenario, string expected)
    {
        scenario.StatusCodeShouldBe(302);
        scenario.AssertThat(new RedirectAssertion(expected, false));
        return scenario;
    }

    /// <summary>
    /// Assert that the http response was redirected permanently
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="expected"></param>
    /// <returns></returns>
    public static Scenario RedirectPermanentShouldBe(this Scenario scenario, string expected)
    {
        scenario.StatusCodeShouldBe(301);
        scenario.AssertThat(new RedirectAssertion(expected, true));
        return scenario;
    }
}