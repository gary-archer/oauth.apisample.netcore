namespace SampleApi.Host.Errors
{
    using System;
    using System.Net;
    using Framework.Api.Base.Errors;
    using SampleApi.Logic.Errors;

    /*
     * A class to translate from application specific business errors to REST client errors
     */
    public class RestErrorTranslator : ApplicationExceptionHandler
    {
        /*
         * The host manages translation from business logic errors to REST 4xx errors
         */
        public override Exception Translate(Exception ex)
        {
            // Catch errors that will be returned with a 4xx status
            if (ex is BusinessError)
            {
                var businessError = (BusinessError)ex;

                // Return a REST specific error
                return ErrorFactory.CreateClientError(
                        this.GetStatusCode(businessError),
                        businessError.ErrorCode,
                        businessError.Message);
            }

            return ex;
        }

        /*
         * Calculate the status code based on the type of business error
         */
        private HttpStatusCode GetStatusCode(BusinessError error)
        {
            switch (error.ErrorCode)
            {
                // Use 404 for these business errors
                case ErrorCodes.CompanyNotFound:
                    return HttpStatusCode.NotFound;

                // Return 400 by default
                default:
                    return HttpStatusCode.BadRequest;
            }
        }
    }
}
