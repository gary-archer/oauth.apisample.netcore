namespace Framework.Base.Errors
{
    using System;

    /*
     * An extended error class so that all unexpected errors have an error code, short message and details
     */
    public sealed class ExtendedException : Exception
    {
        /*
         * The default constructor
         */
        public ExtendedException(string errorCode, string userMessage)
            : this(errorCode, userMessage, null)
        {
        }

        /*
         * The main constructor
         */
        public ExtendedException(string errorCode, string userMessage, Exception ex)
            : base(userMessage, ex)
        {
            this.ErrorCode = errorCode;
        }

        public string ErrorCode { get; private set; }

        public string Details { get; set; }
    }
}