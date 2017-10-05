<!--title: Working with Authentication-->

You may need to do assertions against an endpoint that requires authentication.  This is usually done by issuing a `Challenge`.  Typically you don't need to do anything additonal though Windows Authentication is an exception.  Since Alba is ran through `Kestrel` it does not register a `IAuthenticationHandler` that understands `NTLM` or `Negotiate` authentication schemes.

## Windows Authentication

The endpoint issues a challenge.
<[sample:windows-challenge-endpoint]>

Add a stub authentication handler for Windows and assert the result.
<[sample:asserting-windows-auth]>

You can also add a user to test the result with an authenticated user.
<[sample:asserting-windows-auth-with-user]>

## AuthenticationHandler

You can write your own handler inheriting from `AuthenticationHandler` and adding an extension method to add it to the `Scenario`.
<[sample:stub-windows-auth-handler]>

Add an extention method to add it to the `Scenario`.
<[sample:with-windows-authentication-extension]>
