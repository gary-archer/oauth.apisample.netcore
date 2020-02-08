namespace SampleApi.Logic.Errors
{
    using System;

    /*
     * A 4xx error type that is not REST specific and can be thrown from business logic
     */
    public class BusinessError : Exception
    {
        public BusinessError(string errorCode, string message)
            : base(message)
        {
            this.ErrorCode = errorCode;
        }

        public string ErrorCode { get; private set; }
    }
}
