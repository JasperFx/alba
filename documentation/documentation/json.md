<!--title: Sending and Checking Json-->

## Sending Json

Since posting Json to a web server API is so common, Alba has some helpers for writing Json to the request:

<[sample:sending-json]>


## Reading Json

To deserialize the response body with Json to interrogate the results in a strong typed way, use this syntax:

<[sample:read-json]>

## Customizing Json Serialization

By default, Alba is depending on the near ubiquity of [Json.Net](http://www.newtonsoft.com/json) and uses that for all of its
Json serialization. Because folks frequently customize the serialization, Alba exposes a couple different ways to configure
the Json serialization. In order:

1. If there is a `JsonSerializerSettings` registration in the application's IoC container, Alba uses that
1. Failing that, Alba exposes a `JsonSerializerSettings` property as shown below:

<[sample:customizing-serialization]>

If your application is using an alternative Json serializer, you'll probably need to just subclass `SystemUnderTest`
like this example:

<[sample:custom-system-for-json]>
