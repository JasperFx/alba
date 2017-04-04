<!--title: Sending Http Form Data-->

Posting HTTP form data in the request can be done with the extension method shown below:

<[sample:write-form-data]>

There's a second overload that attempts to use an object and its properties to populate the form data:

<[sample:binding-against-a-model]>

Do note that this only adds first level properties, so if you need to deeper accessors like add "Prop1.Prop2.Prop3,"
you'll have to resort to the dictionary approach.