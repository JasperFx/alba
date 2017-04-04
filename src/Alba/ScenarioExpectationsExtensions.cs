using System.Net;
using Alba.Assertions;

namespace Alba
{
    public static class ScenarioExpectationsExtensions
    {
        // SAMPLE: ContentShouldContain
        public static Scenario ContentShouldContain(this Scenario scenario, string text)
        {
            return scenario.AssertThat(new BodyContainsAssertion(text));
        }
        // ENDSAMPLE

        public static Scenario ContentShouldNotContain(this Scenario scenario, string text)
        {
            return scenario.AssertThat(new BodyDoesNotContainAssertion(text));
        }

        public static Scenario ContentShouldBe(this Scenario scenario, string exactContent)
        {
            return scenario.AssertThat(new BodyTextAssertion(exactContent));
        }

        public static Scenario StatusCodeShouldBeOk(this Scenario scenario)
        {
            return scenario.StatusCodeShouldBe(HttpStatusCode.OK);
        }

        public static Scenario ContentTypeShouldBe(this Scenario scenario, MimeType mimeType)
        {
            return scenario.ContentTypeShouldBe(mimeType.Value);
        }


        public static Scenario ContentTypeShouldBe(this Scenario scenario, string mimeType)
        {
            scenario.Header("content-type").SingleValueShouldEqual(mimeType);
            return scenario;
        }
    }
}