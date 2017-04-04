<!--title: Working with Headers-->

We really didn't worry much about HTTP niceties when I started web programming in the late 90's, but you
can no longer get away with that in today's world. To more correctly use HTTP, Alba comes with some helpers
to deal with HTTP header values.

## Setting Request Headers

To set request headers, you can directly write against the `HttpContext.Request.Headers` collection:

<[sample:setting-request-headers]>

There are also some specific helpers for very common [content negotiation-related](https://en.wikipedia.org/wiki/Content_negotiation) headers:

<[sample:conneg-helpers]>

**Do add extension methods off of the Alba `Scenario` class for common operations in your tests to remove
some of the tedium.**

## Asserting on Expected Response Headers

Alba comes with some out of the box assertions to declaratively check expected header values:

<[sample:asserting-on-header-values]>

You do also have the ability to just interrogate the `HttpContext.Response` in your unit test methods for
anything not covered in the helpers above. To add your own custom assertions, see <[linkto:documentation/assertions]>.