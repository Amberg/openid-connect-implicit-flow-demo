using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

var app = WebApplication.CreateBuilder(args).Build();


/*
 * This Endpoint is hit after startup ---> redirects to Identity server authorize endpoint
 * See: https://openid.net/specs/openid-connect-core-1_0.html#HybridAuthorizationEndpoint
 * Your application should trigger this redirect if the user is not logged in --> ae no auth cookie found (there are libraries for that)
 */
app.MapGet("/", (context) =>
{
	var authorizeEndpoint = "https://dev.idsrv.webaccountplus.com/connect/authorize";
	var param = new Dictionary<string, string>()
	{
		{ "client_id", "dcaBeLocal" },
		{ "redirect_uri", "https://localhost:5100/signin-oidc" },
		{ "response_type", "id_token" },
		{ "scope", "openid profile email" },
		{ "response_mode", "form_post" },
		{ "nonce", "SomValueNeedsToBeValidatedByTheAuthHandler" },

		// here you can encode everything that is required in your handler - ae the redirect URL - often base64 encoded
		{ "state", Convert.ToBase64String(Encoding.UTF8.GetBytes("https://MyPageAfterLogin")) }
	};
	context.Response.Redirect(new Uri(QueryHelpers.AddQueryString(authorizeEndpoint, param)).ToString());
	return Task.CompletedTask;
});


/*
 * After login this handler is called
 * !! This is only a Demo to show how the flow works - this should not be done like this !!
 * There are libraries that
 *
 * Token Validation: See https://openid.net/specs/openid-connect-core-1_0.html#ImplicitIDTValidation
 *
 * - Validates the nonce
 * - Validates the JWT Token
 * - Stores the ID in a Cookie
*/
app.MapPost("/signin-oidc", (context) =>
{
	var idToken = context.Request.Form["id_token"];
	var scope = context.Request.Form["scope"];
	var state = context.Request.Form["state"];

	var jwtToken = new JwtSecurityToken(jwtEncodedString: idToken);

	var initiatorUri = Encoding.UTF8.GetString(Convert.FromBase64String(state));

	// check nonce
	if (jwtToken.Payload.Nonce != "SomValueNeedsToBeValidatedByTheAuthHandler")
	{
		return context.Response.WriteAsync($"Nonce Not valid");
	}
	return context.Response.WriteAsync($"Id Token Received: {jwtToken}\r\n\r\nRedirect To: {initiatorUri}");
	
	// normally your application redirects here to the URL stored in state - the URL that triggered the flow
});

app.Run();