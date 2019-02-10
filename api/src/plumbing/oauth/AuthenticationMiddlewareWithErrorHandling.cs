namespace BasicApi.Plumbing.OAuth
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using BasicApi.Plumbing.Errors;
    using BasicApi.Plumbing.Utilities;

    /*
     * This replaces the default Microsoft middleware so that I can handle errors during introspection
     * It is a little hacky but enables us to reliably distinguish between 401 and 500 for API consumers
     * https://github.com/aspnet/Security/blob/dev/src/Microsoft.AspNetCore.Authentication/AuthenticationMiddleware.cs
     */
    public class AuthenticationMiddlewareWithErrorHandling
    {
        /*
         * Store the inner middleware
         */
        private readonly RequestDelegate next;
        private readonly IAuthenticationSchemeProvider schemeProvider;
        private readonly ILogger logger;

        /*
         * Construct and call the base class
         */
        public AuthenticationMiddlewareWithErrorHandling(RequestDelegate next, IAuthenticationSchemeProvider schemeProvider, ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.schemeProvider = schemeProvider;
            this.logger = loggerFactory.CreateLogger<AuthenticationMiddlewareWithErrorHandling>();
        }

        /*
         * Call the Identity Model handler and handle errors
         * Note that the library does not enable us to access the full introspection response
         */
        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Call the identity model code to validate the access token
                var result = await this.AuthenticateAsync(context);
                if (result?.Principal != null)
                {
                    context.User = result.Principal;
                }

                if (context.User != null && context.User.Identity.IsAuthenticated)
                {
                    // Move to the API middleware if we succeeded
                    await this.next(context);
                }
                else
                {
                    // Check for a particular error string to determine expired tokens
                    if (result.Failure == null || result.Failure.Message.Contains("Token is not active"))
                    {
                        // Return a 401 expired token response to the caller
                        await ResponseErrorWriter.WriteInvalidTokenResponse(context);
                    }
                    else
                    {
                        // Handle failures caught by Identity Model code and return a 500 response indicating failure
                        var handler = new ErrorHandler();
                        var clientError = handler.HandleError(result.Failure, logger);
                        await ResponseErrorWriter.WriteErrorResponse(context, clientError.StatusCode, clientError.ToResponseFormat());
                    }
                }
            }
            catch (Exception exception)
            {
                // Handle failures thrown from Identity Model code and return a 500 response indicating failure
                var handler = new ErrorHandler();
                var clientError = handler.HandleError(exception, logger);
                await ResponseErrorWriter.WriteErrorResponse(context, clientError.StatusCode, clientError.ToResponseFormat());
            }
        }

        /*
         * This repeats the core AuthenticationMiddleware code and triggers the call to the above Invoke method
         */
        private async Task<AuthenticateResult> AuthenticateAsync(HttpContext context)
        {
            AuthenticateResult result = AuthenticateResult.NoResult();

            var defaultAuthenticate = await this.schemeProvider.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                result = await context.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Principal != null)
                {
                    context.User = result.Principal;
                }
            }

            return result;
        }
    }
}
