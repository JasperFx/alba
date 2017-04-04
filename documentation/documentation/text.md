<!--title: Working with Plain Text Requests-->

If you find yourself needing to test HTTP endpoints that either send text or return text, Alba
has you covered with some built in helpers.

## Reading the Response Text

To read the response body as text, use this syntax:

<[sample:read-text]>

## Assertions against the Response Text

You have these built in operations for asserting on the response body text:

<[sample:assert-on-text]>

## Sending Text

Lastly, you can send text to an HTTP endpoint with this syntax:

<[sample:send-text]>

Do note that this also sets the `content-length` header to the string length and
sets the `content-type` header of the request to "text/plain."