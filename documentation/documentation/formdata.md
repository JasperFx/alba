<!--title: Sending Http Form Data-->

Posting HTTP form data in the request can be done with the extension method shown below:

snippet: sample_write_form_data

There's a second overload that attempts to use an object and its properties to populate the form data:

snippet: sample_binding_against_a_model

Do note that this only adds first level properties, so if you need to deeper accessors like add "Prop1.Prop2.Prop3,"
you'll have to resort to the dictionary approach.
