<!--title: Writing Custom Assertions-->

<div class="alert alert-info"><b>Note!</b> All of the assertions are applied during the Scenario execution, and any
failures will be reported out in the Exception message thrown by Alba on Scenario failures.</div>

The Scenario assertions in Alba are completely extensible and you can happily add your own via extension methods - but
please send anything that's generally useful as a pull request to Alba itself;-)

The first step is to write your own implementation of this interface:

snippet: sample_IScenarioAssertion

As an example, here's the assertion from Alba that validates that the response body is supposed to 

snippet: sample_BodyContainsAssertion

Once you have your assertion class, you can apply it to a scenario through an extension method against the 
`Scenario` class. Here's the `Scenario.ContentShouldContain(text)` implementation from Alba itself:

snippet: sample_ContentShouldContain

Finally, use your new asssertion in a Scenario like this:

snippet: sample_using_ContentShouldBe
