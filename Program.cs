using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.WebUtilities;

var app = WebApplication.CreateBuilder(args).Build();


/*
 * This Endpoint is hit after startup ---> redirects to Identity server authorize endpoint
 * See: https://openid.net/specs/openid-connect-core-1_0.html#HybridAuthorizationEndpoint
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

	var jwtToken = new JwtSecurityToken(jwtEncodedString: idToken);

	// check nonce
	if (jwtToken.Payload.Nonce != "SomValueNeedsToBeValidatedByTheAuthHandler")
	{
		return context.Response.WriteAsync($"Nonce Not valid");
	}
	return context.Response.WriteAsync($"Id Token Received: {jwtToken}");
});

app.Run();