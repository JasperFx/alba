<!--title: Working with Plain Text Requests-->

If you find yourself needing to test HTTP endpoints that either send text or return text, Alba
has you covered with some built in helpers.

## Reading the Response Text

To read the response body as text, use this syntax:

snippet: sample_read_text

## Assertions against the Response Text

You have these built in operations for asserting on the response body text:

snippet: sample_assert_on_text

## Sending Text

Lastly, you can send text to an HTTP endpoint with this syntax:

snippet: sample_send_text

Do note that this also sets the `content-length` header to the string length and
sets the `content-type` header of the request to "text/plain."
